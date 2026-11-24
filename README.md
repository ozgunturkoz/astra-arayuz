# Astra Ground Station 🚀📡

Real-time ground station for **Astra Rocket Team**'s TEKNOFEST rocket.
It receives telemetry from the rocket and the payload, plots and maps it live, drives a 3D visualization,
and forwards data to the competition's referee ground station.

![Ground station UI](astra%20aray%C3%BCz.PNG)

## Features
- **Multi-link serial telemetry.** Separate serial ports for the rocket, the payload and the referee ground station (HYİ).
- **Telemetry parsing.** Comma-separated frames are parsed into GPS lat/lon/alt, barometric pressure and altitude,
  3-axis gyro and accelerometer, attitude angle and the flight-computer state.
- **Live charts.** Altitude, attitude angle and other channels are plotted in real time with auto-scaling axes.
- **Live maps.** Rocket and payload GPS positions are shown on two separate maps with GMap.NET.
- **3D orientation.** Attitude data is streamed over **UDP** to a Unity 3D visualization (launched and closed automatically by the app).
- **Referee packet.** Builds the fixed-length **78-byte binary competition packet** with float-to-byte packing and a **checksum**.

## Tech stack
C# · .NET Framework (WinForms) · System.IO.Ports · UDP sockets · GMap.NET · MS Chart · Unity (3D view)

## Architecture
```
 Rocket flight computer ──RF/serial──┐
 Payload computer ───────RF/serial──┤──► Astra Ground Station ──► live charts + GPS maps
                                      │        │
                                      │        ├──UDP──► Unity 3D orientation view
                                      │        └──serial──► Referee ground station (78-byte packet + checksum)
```

## Running
1. Open `Astra Ground Station/arayüz okuma.sln` in Visual Studio.
2. Build and run, select the COM ports for the rocket, payload and HYİ links, then press **Connect**.
3. The Unity 3D viewer is a separate build; the app starts it automatically if it is present.

## Context
Developed as part of the Astra Rocket Team avionics software for TEKNOFEST.
The team placed **5th + Best Design Award (2024)**, **7th (2025)** and **9th (2026)** in the TEKNOFEST Rocket Competition.

Author: **Özgün Türköz** · [LinkedIn](https://linkedin.com/in/ozgunturkozz)
