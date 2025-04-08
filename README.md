# Memory Game

## Overview
Memory Game is a classic card matching game implemented in C# and WPF (Windows Presentation Foundation) using the MVVM (Model-View-ViewModel) architectural pattern and data binding. The game challenges players to match pairs of cards by turning them over and remembering their positions.

## Requirements
- .NET 8.0
- Windows operating system

## Features
- **User Authentication System**
  - Create new user accounts with custom profile pictures
  - Sign in with existing accounts
  - Delete user accounts

- **Game Modes**
  - Standard mode (4x4 grid)
  - Custom mode (configurable MxN grid, where M and N range from 2 to 6)

- **Image Categories**
  - Three predefined image categories to choose from

- **Game Features**
  - Configurable time limit
  - Game state preservation (save & load functionality)
  - Random card distribution for each new game

- **Statistics**
  - Track games played and won for each user
  - View statistics for all users

## How to Play
1. Sign in with an existing account or create a new one
2. Select a game mode (Standard or Custom)
3. Choose an image category
4. Start a new game
5. Click on cards to flip them over
6. Match pairs of identical cards before the time runs out

## Implementation Details
- Built using WPF and C#
- Implements MVVM design pattern
- Uses data binding for UI updates
- Implements ICommand for action handling
- User data, game states, and statistics are stored in files

## Project Structure
- User authentication system
- Game logic implementation
- File management for saving/loading games and user data
- Statistics tracking system

## Screenshots
![Screenshot 2025-04-08 131714](https://github.com/user-attachments/assets/91202eb5-d620-4c6a-afe0-8a1329058585)
![Screenshot 2025-04-08 131800](https://github.com/user-attachments/assets/5c4eeb3e-a81f-49db-90c1-a589d16ce20e)
![Screenshot 2025-04-08 131908](https://github.com/user-attachments/assets/315b5082-67e0-43e6-950d-d7749c341179)


## Installation
1. Ensure you have .NET 8.0 installed on your system
2. Clone this repository
3. Open the solution in Visual Studio
4. Build and run the application

## Development Notes
This application was developed as part of a university project with a focus on implementing the MVVM pattern and data binding in WPF applications.
