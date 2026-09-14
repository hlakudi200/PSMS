# PSMS Mobile

The PSMS mobile client is an Expo-managed React Native application.

## Run locally

```bash
cd mobile
npm install
npm run start
```

Use the Expo developer tools to open the app on an Android emulator, iOS simulator, web browser, or a device running Expo Go.

Before running on a physical device, copy `.env.example` to `.env` and set `EXPO_PUBLIC_API_BASE_URL` to your development computer's LAN IP. `localhost` points to the phone itself when using Expo Go.

## Available scripts

- `npm run start` — start the Expo development server.
- `npm run android` — open the Android target.
- `npm run ios` — open the iOS target (requires macOS).
- `npm run web` — run the web target.

## Next integration steps

Authentication uses the same ABP flow as the web app: tenant availability, token authentication, JWT role decoding, and current-session lookup. Tokens and tenant metadata are stored with Expo SecureStore. Only the `Student` and `Parent` roles can establish a mobile session.
