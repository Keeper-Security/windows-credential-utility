# Windows Credential Utility

This utility interacts with the native windows APIs to store and retrieve secrets from Windows Credential Manager. It can be used by any integration, plugin, or code base to store and retrieve credentials, secrets, and passwords in the Windows Credential Manager simply and natively.

To use this utility, you can deploy the pre-built binary from the releases page, or by importing it into your code base. Both use cases are covered below.

For Linux implementations, see the [Linux Keyring Utility](https://github.com/Keeper-Security/linux-keyring-utility).

## Using the Executable

Download the latest version from the releases page and optionally add it to PATH to get started.

### Usage

The executable supports two commands:

1. `set`
2. `get`

Both commands require an application `name` (i.e. the name of the credential in / to be stored in the Windows Credential Manager) as the first argument.

### `set`

`set` requires a second argument of the secret to be stored. This can be either a:

1. BASE64 string
2. JSON string
3. Path to an existing JSON file

When the secret is saved to Windows Credential Manager it is first encoded into a BASE64 format (if not already a BASE64 string). This standardizes the format for both consistent storage and to make it easier to consume by Keeper integrations and products. 

> If you need a support for a different format, please submit a feature request. We'd be happy to extend this to support other use cases.

### `get`

`get` returns the stored BASE64 encoded config to `stdout` and exits with a `0` exit code. The requesting integration can capture the output for consumption. Any errors encountered retrieving the config will return an `non-zero` exit code and write to `stderr`.

### Example

```shell
# Save a secret
wcu set APPNAME eyJ1c2VybmFtZSI6ICJnb2xsdW0iLCAicGFzc3dvcmQiOiAiTXlQcmVjaW91cyJ9
# or
wcu set APPNAME config.json

# Retrieve a secret
wcu get APPNAME
```

## Contributing

Please read and refer to the contribution guide before making your first PR.

For bugs, feature request, etc., please submit an issue!
