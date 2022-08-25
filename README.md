# AlphaPaulloz

## How to generate an access and a refresh token

First, ask for an authorization code.
```
GET https://id.twitch.tv/oauth2/authorize
        ?response_type=code
        &client_id=<CLIENT_ID>
        &redirect_uri=http://localhost
        &scope=chat:read+chat:edit
```

Then, use the obtained code to ask for an access token.
```
POST https://id.twitch.tv/oauth2/token
        ?client_id=<CLIENT_ID>
        &client_secret=<CLIENT_SECRET>
        &code=<AUTHORIZATION_CODE>
        &grant_type=authorization_code
        &redirect_uri=http://localhost
```
The response should contain both an access and a refresh token.
