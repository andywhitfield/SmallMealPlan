using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SmallMealPlan.Data;
using SmallMealPlan.Model;

namespace SmallMealPlan.Web.Authorisation;

public class AuthorisationHandler(ILogger<AuthorisationHandler> logger,
    IUserAccountRepository userAccountRepository, IFido2 fido2)
    : IAuthorisationHandler
{
    public async Task<(bool IsReturningUser, string VerifyOptions)> HandleSigninRequest(string email, CancellationToken cancellationToken)
    {
        UserAccount? user;
        string options;
        if ((user = await userAccountRepository.GetUserAccountByEmailAsync(email)) != null)
        {
            logger.LogTrace($"Found existing user account with email [{email}], creating assertion options");
            options = fido2.GetAssertionOptions(new()
            {
                AllowedCredentials =
                    await userAccountRepository
                        .GetUserAccountCredentialsAsync(user)
                        .Select(uac => new PublicKeyCredentialDescriptor(uac.CredentialId))
                        .ToArrayAsync(cancellationToken: cancellationToken),
                UserVerification = UserVerificationRequirement.Discouraged
            }).ToJson();
        }
        else
        {
            logger.LogTrace($"Found no user account with email [{email}], creating request new creds options");
            options = fido2.RequestNewCredential(new()
            {
                User = new Fido2User() { Id = Encoding.UTF8.GetBytes(email), Name = email, DisplayName = email },
                PubKeyCredParams = [],
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None
            }).ToJson();
        }

        logger.LogTrace($"Created sign in options: {options}");

        return (user != null, options);        
    }

    public async Task<bool> HandleSigninVerifyRequest(HttpContext httpContext, string email, string verifyOptions, string verifyResponse, CancellationToken cancellationToken)
    {
        UserAccount? user;
        if ((user = await userAccountRepository.GetUserAccountByEmailAsync(email)) != null)
        {
            if (!await SigninUserAsync(user, verifyOptions, verifyResponse, cancellationToken))
                return false;
        }
        else
        {
            user = await CreateNewUserAsync(email, verifyOptions, verifyResponse, cancellationToken);
            if (user == null)
                return false;
        }

        List<Claim> claims = [new Claim(ClaimTypes.Name, user.Email!)];
        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        AuthenticationProperties authProperties = new() { IsPersistent = true };
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        logger.LogTrace($"Signed in: {email}");

        return true;
    }

    private async Task<UserAccount?> CreateNewUserAsync(string email, string verifyOptions, string verifyResponse, CancellationToken cancellationToken)
    {
        logger.LogTrace("Creating new user credientials");
        var options = CredentialCreateOptions.FromJson(verifyOptions);

        AuthenticatorAttestationRawResponse? authenticatorAttestationRawResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(verifyResponse);
        if (authenticatorAttestationRawResponse == null)
        {
            logger.LogWarning($"Cannot parse signin verify response: {verifyResponse}");
            return null;
        }

        logger.LogTrace($"Successfully parsed response: {verifyResponse}");

        var success = await fido2.MakeNewCredentialAsync(new()
        {
            AttestationResponse = authenticatorAttestationRawResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = (_, _) => Task.FromResult(true)
        }, cancellationToken: cancellationToken);
        logger.LogInformation($"got success status: {success}");
        if (success == null)
        {
            logger.LogWarning("Could not create new credential");
            return null;
        }

        logger.LogTrace($"Got new credential: {JsonSerializer.Serialize(success)}");

        return await userAccountRepository.CreateNewUserAsync(email, success.Id,
            success.PublicKey, success.User.Id);
    }

    private async Task<bool> SigninUserAsync(UserAccount user, string verifyOptions, string verifyResponse, CancellationToken cancellationToken)
    {
        logger.LogTrace($"Checking credientials: {verifyResponse}");
        AuthenticatorAssertionRawResponse? authenticatorAssertionRawResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(verifyResponse);
        if (authenticatorAssertionRawResponse == null)
        {
            logger.LogWarning($"Cannot parse signin assertion verify response: {verifyResponse}");
            return false;
        }
        var options = AssertionOptions.FromJson(verifyOptions);
        var userAccountCredential = await userAccountRepository.GetUserAccountCredentialsAsync(user).FirstOrDefaultAsync(uac => uac.CredentialId.SequenceEqual(authenticatorAssertionRawResponse.RawId), cancellationToken);
        if (userAccountCredential == null)
        {
            logger.LogWarning($"No credential id [{Convert.ToBase64String(authenticatorAssertionRawResponse.RawId)}] for user [{user.Email}]");
            return false;
        }
        
        logger.LogTrace($"Making assertion for user [{user.Email}]");
        var res = await fido2.MakeAssertionAsync(new()
        {
            AssertionResponse = authenticatorAssertionRawResponse,
            OriginalOptions = options,
            StoredPublicKey = userAccountCredential.PublicKey,
            StoredSignatureCounter = userAccountCredential.SignatureCount,
            IsUserHandleOwnerOfCredentialIdCallback = VerifyExistingUserCredentialAsync
        }, cancellationToken: cancellationToken);
        if (res == null)
        {
            logger.LogWarning("Signin assertion failed");
            return false;
        }

        logger.LogTrace($"Signin success, got response: {JsonSerializer.Serialize(res)}");
        await userAccountRepository.SetSignatureCountAsync(userAccountCredential, res.SignCount);

        return true;
    }

    private async Task<bool> VerifyExistingUserCredentialAsync(IsUserHandleOwnerOfCredentialIdParams credentialIdUserHandleParams, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Checking credential {credentialIdUserHandleParams.CredentialId} - {credentialIdUserHandleParams.UserHandle}");
        var userAccountCredentials = await userAccountRepository.GetUserAccountCredentialsByUserHandleAsync(credentialIdUserHandleParams.UserHandle);
        return userAccountCredentials?.CredentialId.SequenceEqual(credentialIdUserHandleParams.CredentialId) ?? false;
    }
}
