# SpaceX Mission Tracker & Roadster Simulator

A Unity-based mobile application showcasing real-time SpaceX launch data visualization and an interactive orbital mechanics simulation of Starman's Tesla Roadster journey through space.

## SpaceX Launch Browser

- **Real-time Data Integration**: Fetches live data from SpaceX API v4
- **Advanced Filtering**: Filter launches by status (past/upcoming) with search functionality
- **Detailed Mission Information**: View comprehensive details including:
  - Mission objectives and payload information
  - Rocket specifications and country of origin
  - Support ships with photo galleries
  - Launch dates and success status
- **Performance Optimized**: Implements object pooling for smooth scrolling through hundreds of launches
- **Intelligent Caching**: Reduces API calls and improves load times

## Tesla Roadster Orbital Simulator

- **Scientifically Accurate**: Real orbital mechanics calculations based on NASA JPL Horizons data
- **Two Simulation Modes**:
  - **Required Mode**: Day-by-day progression from Feb 2018 to Oct 2019
  - **Optional Mode**: Extended timeline with intelligent interpolation for sparse data points
- **Interactive 3D Visualization**:
  - Orbital trail rendering showing Roadster's path
  - Touch/mouse-controlled camera with zoom and rotation
  - Real-time orbital element display
  - Accurate scale representation of the solar system
- **Custom Orbital Calculator**: Implements Kepler's laws for precise position calculations
