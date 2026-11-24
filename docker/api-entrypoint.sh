#!/bin/sh
set -eu

# The development VM mounts the IdP CA in App_Data.  Install it at startup so
# recreating the container does not silently lose trust in the IdP JWKS endpoint.
if [ -s /app/App_Data/edan-idp-rootCA.crt ]; then
    install -m 0644 /app/App_Data/edan-idp-rootCA.crt \
        /usr/local/share/ca-certificates/edan-idp-rootCA.crt
    update-ca-certificates >/dev/null
fi

exec dotnet EDAN.Tensor.ApiHost.dll
