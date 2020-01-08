# Introduction
This repo was created to explain and understand the possibile utilization of Azure Key Vault in the following scenario: 

Customer has an application that performs the following: 
- Read sensitive data from a secure external store (i.e. SQL Server)
- Signs the sensitive data, using PGP or X509 certificates
- Share the sensitive data + signature with the application's consumers

In this repo, the focus will be on signing the sensitive data. Other elements of this question, such as securing SQL Server and/or managing a PKI infrastructure to revoke certificates will be out of scope. 


# Concepts
## Azure Key Vault - Keys, Secrets, Certificates
Azure Key Vault can be used to store and work with different types of data (https://docs.microsoft.com/en-us/azure/key-vault/about-keys-secrets-and-certificates):
* **Cryptographic keys**: Supports multiple key types and algorithms, and enables the use of Hardware Security Modules (HSM) for high value keys.
* **Secrets**: Provides secure storage of secrets, such as passwords and database connection strings.
* **Certificates**: Supports certificates, which are built on top of keys and secrets and add an automated renewal feature.
* **Azure Storage**: Can manage keys of an Azure Storage account for you. Internally, Key Vault can list (sync) keys with an Azure Storage Account, and regenerate (rotate) the keys periodically.

A more elaborate explanation can be found in the above link. Key takeaways: 
* When you add a certificates, this automatically creates a key and secret value
* Azure Key Vault can integrate with existing Certificate Authorities [tutorial](https://docs.microsoft.com/en-us/azure/key-vault/certificate-scenarios) 

## Supported key and certificate types
* Azure Key Vault supports direct import of RSA and Elliptic Curve keys (https://docs.microsoft.com/en-us/azure/key-vault/about-keys-secrets-and-certificates#key-vault-keys), Key Vault supports RSA keys of sizes 2048, 3072 and 4096. Key Vault supports Elliptic Curve key types P-256, P-384, P-521, and P-256K (SECP256K1).
* Azure Key Vault provides support for management of x509 certificates and the following behaviors [link](https://docs.microsoft.com/en-us/azure/key-vault/about-keys-secrets-and-certificates#key-vault-certificates):  
  - Allows a certificate owner to create a policy that directs Key Vault to manage the life-cycle of a certificate.
  - Allows certificate owners to provide contact information for notification about life-cycle events of expiration and renewal of certificate.
  - Supports automatic renewal with selected issuers - Key Vault partner X509 certificate providers / certificate authorities.

## Signing a message
Azure Key Vault can be used to sign specific data, without exposing the certificate / key used for signing itself. 
This operation is accessible via the KeyVault SDK or via the Rest API, and depends on a Key value being present in AKV: https://docs.microsoft.com/en-us/rest/api/keyvault/sign/sign
This Key value is automatically created when adding a certificate, or can be imported directly by an authorized user or automated process. 

An example implementation, using _RSASSA-PKCS1-v1_5 using SHA-512_ is provided. Other signing algorithms are supported; a full list can be found at the following link: 
https://docs.microsoft.com/en-us/rest/api/keyvault/sign/sign#jsonwebkeysignaturealgorithm

# Approach
The infrastructure required for this setup is as follows: 
* Create an App Registration in Azure AD (unless you plan to use Azure's Managed Identity)
* Create a resource group
* Create an Azure Key Vault in said resource group
* Define an Azure Key Vault Access Policy for the registered application, granting sufficient access (at least Key/Sign)

If this is done correctly, the example application can be used to sign sensitive content. 


