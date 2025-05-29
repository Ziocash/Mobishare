# 🚲 Mobishare
**A smart and green solution to move around the city.**
**MobiShare** is a smart urban mobility platform that lets you get around the city easily and sustainably using **standard bikes**, **electric bikes**, and **electric scooters**. With MobiShare, you can skip the traffic, reduce your carbon footprint, and move freely through the city.

---

## 🚀 Features

- 🗺 **Live map** showing available vehicles in real time
- ⚡ **Battery level monitoring** (for e-bikes and scooters)  
- 🅿️ **Smart parking assistance** with geofenced zones  
- 🔄 **Ride history & payment tracking**  
- 💳 **In-app payments** with cards or digital wallets  
- 🎫 **Passes & promotions** for frequent users  

---

## 🚘 Available Vehicles

| 🚴‍♂️ Vehicle               | 📋 Description                                                               |
|--------------------------|------------------------------------------------------------------------------|
| **Standard Bicycle**     | Classic bike without motor assistance — great for short rides and exercise   |
| **Electric Bicycle**     | Pedal-assist bike, perfect for longer distances or hilly routes              |
| **Electric Scooter**     | Fast and flexible option for busy urban streets                              |

---

## 🧑‍💻 Tech Stack

- **Frontend**: Razor Pages, CSS  
- **Backend**: ASP .NET Core
- **Database**: SQLite
- **Real-time Communication**: SignalR
- **Authentication**: Google OAuth 2.0
- **Payments**: PayPal SDK
- **Messaging**: MQTT (for vehicle telemetry)
- **Mapping & GPS**: Google Maps SDK  
- **Cloud Services**: AWS
  
---

## 🧾 Required Environment Variables / App Settings

Configure the following variables in your `appsettings.Development.json`, `appsettings.Production.json`, or as environment variables.

### 🔑 Required Keys

- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`
- `GoogleMaps:ApiKey`
- `Payments:PayPal:ClientId`
- `Payments:PayPal:ClientSecret`
- `Payments:PayPal:PayPalUrl` (e.g. `https://api.sandbox.paypal.com` or `https://api.paypal.com`)

---

### 🧪 Example `appsettings.Development.json`

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "GoogleMaps": {
    "ApiKey": "your-google-maps-api-key"
  },
  "Payments": {
    "PayPal": {
      "ClientId": "your-paypal-client-id",
      "ClientSecret": "your-paypal-client-secret",
      "PayPalUrl": "https://api.sandbox.paypal.com"
    }
  }
}
```

---

## ⚙️ Getting Started

Follow these steps to run the project locally.

---

### 📥 1. Clone the repository

```bash
git clone https://github.com/your-username/mobishare.git
cd mobishare
```

### 🔧 2. Configure environment variablesClone the repository
Make sure to set up the required settings either in:
- `appsettings.Development.json` (recommended for local development), or
system environment variables.
- Refer to the [Required Environment Variables / App Settings](#-required-environment-variables--app-settings) section for details.

### 📦 3. Restore dependencies
Restore NuGet packages and all dependencies:
`dotnet restore`

### 🚀 4. Run the backend
You are ready to go! Run:
`dotnet run --project Mobishare`

