# Healthcare Disease Detection Platform

## Project Overview
A comprehensive healthcare platform for detecting and predicting multiple medical conditions including heart disease, diabetes, and general symptoms diagnosis. The system provides personalized health insights, treatment recommendations, and precautionary measures based on patient data.

## Features
- Multi-disease detection system (Heart Disease, Diabetes, Symptom-based diagnosis)
- User account management with personalized health profiles
- Secure API integration between ML models and web application
- Historical tracking of health predictions
- Detailed disease information with prevention recommendations
- Responsive design for cross-platform accessibility

## Technologies Used
- **Backend**: ASP.NET Core 8.0, Entity Framework Core, C#
- **Machine Learning**: Python, Scikit-learn, Pandas/NumPy, Jupyter Notebooks
- **Frontend**: ASP.NET Razor Pages, HTML/CSS/JavaScript, Bootstrap
- **Database**: SQL Server
- **Others**: RESTful API, Email Services, Authentication

## Project Structure
- **Ai Models**: Contains Jupyter notebooks and trained ML models
  - Diabetes prediction
  - Heart disease detection
  - Symptom checker
- **Api**: Python API for model inference
- **NewProject**: ASP.NET Core web application
  - Controllers for handling requests
  - Models for data representation
  - Views for user interface
  - Services for business logic

## Getting Started
### Prerequisites
- .NET 8.0 SDK
- Python 3.10+
- SQL Server
- Visual Studio 2022 (recommended)

### Installation
1. Clone the repository
2. Set up the database using Entity Framework migrations
3. Install required Python packages
4. Run the ASP.NET Core application
