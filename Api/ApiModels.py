from fastapi import FastAPI, Depends, HTTPException, Header, Request, Form
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field, validator, root_validator
from typing import List, Optional, Dict, Any
import joblib
import pandas as pd
import numpy as np
import os
from pathlib import Path
import logging
from contextlib import asynccontextmanager
import re

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("api_logs.log")
    ]
)
logger = logging.getLogger(__name__)

# Define base directory
BASE_DIR = Path(__file__).resolve().parent

# Model paths configuration
MODEL_PATHS = {
    "heart": {
        "model": BASE_DIR / "HeartModel" / "heart_disease_model.pkl",
        "preprocessor": BASE_DIR / "HeartModel" / "heart_disease_preprocessor.pkl"
    },
    "diabetes": {
        "model": BASE_DIR / "DiabetsModel" / "best_diabetes_model.pkl",
        "scaler": BASE_DIR / "DiabetsModel" / "scaler_diabetes.pkl"
    },
    "symptom": {
        "model": BASE_DIR / "SymptomChecker" / "disease_prediction_model.pkl",
        "encoder": BASE_DIR / "SymptomChecker" / "disease_encoder.pkl",
        "symptoms": BASE_DIR / "SymptomChecker" / "all_symptoms.pkl",
        "descriptions": BASE_DIR / "SymptomChecker" / "data" / "symptom_Description.csv",
        "precautions": BASE_DIR / "SymptomChecker" / "data" / "symptom_precaution.csv"
    }
}

# Global model containers
models = {
    "heart": {"model": None, "preprocessor": None},
    "diabetes": {"model": None, "scaler": None},
    "symptom": {
        "model": None, 
        "encoder": None, 
        "all_symptoms": None,
        "descriptions": None,
        "precautions": None
    }
}

# Model loading functions
def load_model(model_type: str) -> bool:
    """Generic model loader function"""
    try:
        if model_type == "heart":
            models["heart"]["model"] = joblib.load(MODEL_PATHS["heart"]["model"])
            models["heart"]["preprocessor"] = joblib.load(MODEL_PATHS["heart"]["preprocessor"])
            logger.info("Heart disease model loaded successfully")
            
        elif model_type == "diabetes":
            models["diabetes"]["model"] = joblib.load(MODEL_PATHS["diabetes"]["model"])
            models["diabetes"]["scaler"] = joblib.load(MODEL_PATHS["diabetes"]["scaler"])
            logger.info("Diabetes model loaded successfully")
            
        elif model_type == "symptom":
            models["symptom"]["model"] = joblib.load(MODEL_PATHS["symptom"]["model"])
            models["symptom"]["encoder"] = joblib.load(MODEL_PATHS["symptom"]["encoder"])
            models["symptom"]["all_symptoms"] = joblib.load(MODEL_PATHS["symptom"]["symptoms"])
            models["symptom"]["descriptions"] = pd.read_csv(MODEL_PATHS["symptom"]["descriptions"])
            models["symptom"]["precautions"] = pd.read_csv(MODEL_PATHS["symptom"]["precautions"])
            logger.info("Symptom checker model loaded successfully")
            
        return True
    except FileNotFoundError as e:
        logger.error(f"File not found when loading {model_type} model: {str(e)}")
        return False
    except Exception as e:
        logger.error(f"Error loading {model_type} model: {str(e)}")
        return False

# Check if all required files exist
def check_files_exist():
    missing_files = []
    
    # Check heart model files
    for key, path in MODEL_PATHS["heart"].items():
        if not path.exists():
            missing_files.append(str(path))
    
    # Check diabetes model files
    for key, path in MODEL_PATHS["diabetes"].items():
        if not path.exists():
            missing_files.append(str(path))
    
    # Check symptom model files
    for key, path in MODEL_PATHS["symptom"].items():
        if not path.exists():
            missing_files.append(str(path))
    
    return missing_files

# Lifespan event handler (modern approach replacing on_event)
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: Check files and load models
    missing_files = check_files_exist()
    if missing_files:
        logger.error(f"Missing files: {', '.join(missing_files)}")
        logger.error("Some models may not function correctly due to missing files")
    
    # Try to load all models
    load_model("heart")
    load_model("diabetes")
    load_model("symptom")
    
    yield  # Server is running during this yield
    
    # Shutdown: No cleanup needed for our models

# Initialize FastAPI app with lifespan
app = FastAPI(
    title="Disease Detection API",
    description="API for heart disease, diabetes, and symptom-based disease prediction",
    version="1.0.0",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allow all origins
    allow_credentials=True,
    allow_methods=["*"],  # Allow all methods
    allow_headers=["*"],  # Allow all headers
)

# API key (simple implementation)
# In development, we'll make it optional to simplify testing
API_KEY = "your-disease-detection-api-key-2023"

def validate_api_key(x_api_key: str = Header(None)):
    # During development, allow requests without API key
    if x_api_key is None:
        return None
    
    if x_api_key != API_KEY:
        raise HTTPException(status_code=401, detail="Invalid API key")
    return x_api_key

# Define Pydantic models for request/response with proper validation ranges
class HeartDiseaseRequest(BaseModel):
    age: int = Field(..., ge=25, le=90, description="Age in years")
    sex: int = Field(..., ge=0, le=1, description="Sex (0=female, 1=male)")
    cp: int = Field(..., ge=0, le=3, description="Chest pain type (0=Typical Angina, 1=Atypical Angina, 2=Non-anginal Pain, 3=Asymptomatic)")
    trestbps: int = Field(..., ge=90, le=200, description="Resting blood pressure (mm Hg)")
    chol: int = Field(..., ge=120, le=570, description="Serum cholesterol (mg/dl)")
    fbs: int = Field(..., ge=0, le=1, description="Fasting blood sugar > 120 mg/dl (1=true, 0=false)")
    restecg: int = Field(..., ge=0, le=2, description="Resting ECG (0=Normal, 1=ST-T Wave Abnormality, 2=Left Ventricular Hypertrophy)")
    thalach: int = Field(..., ge=60, le=220, description="Maximum heart rate achieved")
    exang: int = Field(..., ge=0, le=1, description="Exercise induced angina (1=yes, 0=no)")
    oldpeak: float = Field(..., ge=0, le=6.0, description="ST depression induced by exercise relative to rest")
    slope: int = Field(..., ge=0, le=2, description="Slope of peak exercise ST segment (0=Upsloping, 1=Flat, 2=Downsloping)")
    ca: int = Field(..., ge=0, le=3, description="Number of major vessels colored by fluoroscopy (0-3)")
    thal: int = Field(..., ge=1, le=3, description="Thalassemia (1=Fixed Defect, 2=Normal, 3=Reversible Defect)")
    
    @validator('age', 'sex', 'cp', 'trestbps', 'chol', 'fbs', 'restecg', 'thalach', 'exang', 'slope', 'ca', 'thal', pre=True)
    def validate_integer(cls, value):
        if isinstance(value, str):
            try:
                return int(value)
            except ValueError:
                raise ValueError(f"Invalid integer value: {value}")
        return value
    
    @validator('oldpeak', pre=True)
    def validate_float(cls, value):
        if isinstance(value, str):
            try:
                return float(value)
            except ValueError:
                raise ValueError(f"Invalid float value: {value}")
        return value

class HeartDiseaseResponse(BaseModel):
    prediction: str
    probability: float
    status: str = "success"
    request_data: Optional[Dict[str, Any]] = None

class DiabetesRequest(BaseModel):
    Gender: int = Field(..., ge=0, le=1, description="Gender (0=Male, 1=Female)")
    AGE: int = Field(..., ge=1, le=119, description="Age in years")
    Urea: float = Field(..., gt=0, description="Blood urea nitrogen level")
    Cr: float = Field(..., gt=0, description="Creatinine level")
    HbA1c: float = Field(..., gt=0, le=15, description="Hemoglobin A1c level")
    Chol: int = Field(..., gt=0, le=799, description="Total cholesterol level")
    TG: int = Field(..., gt=0, le=999, description="Triglycerides level")
    HDL: int = Field(..., gt=0, le=199, description="High-density lipoprotein level")
    LDL: int = Field(..., gt=0, le=599, description="Low-density lipoprotein level")
    VLDL: float = Field(..., gt=0, le=199.9, description="Very low-density lipoprotein level")
    BMI: float = Field(..., gt=0, le=80, description="Body Mass Index")
    
    # Add validation to convert strings to numbers if needed
    @validator('Gender', 'AGE', 'Chol', 'TG', 'HDL', 'LDL', pre=True)
    def validate_integer(cls, value):
        if isinstance(value, str):
            try:
                return int(value)
            except ValueError:
                raise ValueError(f"Invalid integer value: {value}")
        return value
    
    @validator('Urea', 'Cr', 'HbA1c', 'VLDL', 'BMI', pre=True)
    def validate_float(cls, value):
        if isinstance(value, str):
            try:
                return float(value)
            except ValueError:
                raise ValueError(f"Invalid float value: {value}")
        return value

class DiabetesResponse(BaseModel):
    original_class: str  # N, P, or Y
    binary_result: str  # "Positive" or "Negative"
    risk_level: str  # "Low", "Medium", "High", or "Negative"
    clinical_status: str  # "Non-diabetic", "Pre-diabetic", or "Diabetic"
    probabilities: Dict[str, float]  # {"N": 0.xx, "P": 0.xx, "Y": 0.xx}
    recommendation: str
    key_indicators: Dict[str, float]  # {"HbA1c": x.x, "BMI": x.x}
    status: str = "success"
    request_data: Optional[Dict[str, Any]] = None
    
    class Config:
        schema_extra = {
            "example": {
                "original_class": "P",
                "binary_result": "Negative",
                "risk_level": "High",
                "clinical_status": "Pre-diabetic",
                "probabilities": {"N": 0.15, "P": 0.78, "Y": 0.07},
                "recommendation": "Immediate lifestyle intervention. Follow-up in 3 months.",
                "key_indicators": {"HbA1c": 5.8, "BMI": 23.0},
                "status": "success",
                "request_data": {"Gender": 0, "AGE": 45, "HbA1c": 5.8, "BMI": 23.0}
            }
        }

class SymptomRequest(BaseModel):
    symptoms: List[str]
    top_n: Optional[int] = 3
    
    @validator('symptoms')
    def validate_symptoms(cls, symptoms):
        if not symptoms or len(symptoms) == 0:
            raise ValueError("At least one symptom must be provided")
        return symptoms
    
    @validator('top_n')
    def validate_top_n(cls, top_n):
        if top_n < 1 or top_n > 10:
            return 3  # Default to 3 if out of range
        return top_n

class PredictionItem(BaseModel):
    disease: str
    probability: float
    description: str
    precautions: List[str]

class SymptomResponse(BaseModel):
    predictions: List[PredictionItem]
    status: str = "success"
    request_data: Optional[Dict[str, Any]] = None

class ErrorResponse(BaseModel):
    error: str
    status: str = "error"

# DIABETES RISK ASSESSMENT FUNCTION
def predict_diabetes_with_risk(patient_data, model=None, scaler=None):
    """
    Predicts diabetes status with detailed risk level assessment
    
    Args:
        patient_data: Dictionary with patient features
        model: Pre-loaded model (optional)
        scaler: Pre-loaded scaler (optional)
    
    Returns:
        dict: Complete assessment with prediction class, risk level, and probability
    """
    # Convert patient data to DataFrame
    df = pd.DataFrame([patient_data])
    
    # Scale the input data
    df_scaled = pd.DataFrame(
        scaler.transform(df), 
        columns=df.columns
    )
    
    # Get prediction
    prediction = model.predict(df_scaled)[0]
    
    # Get probabilities if available
    if hasattr(model, 'predict_proba'):
        probabilities = model.predict_proba(df_scaled)[0]
        
        # Extract probabilities for each class
        class_indices = {cls: idx for idx, cls in enumerate(model.classes_)}
        n_prob = probabilities[class_indices['N']] if 'N' in class_indices else 0
        p_prob = probabilities[class_indices['P']] if 'P' in class_indices else 0
        y_prob = probabilities[class_indices['Y']] if 'Y' in class_indices else 0
    else:
        # If model doesn't support probabilities, use dummy values
        n_prob = 0.33 if prediction == 'N' else 0
        p_prob = 0.33 if prediction == 'P' else 0
        y_prob = 0.33 if prediction == 'Y' else 0
    
    # Clinical values
    hba1c = patient_data['HbA1c']
    bmi = patient_data['BMI']
    
    # Determine risk level based on classification and clinical values
    if prediction == 'Y':
        # Positive case (has diabetes)
        binary_result = "Positive"
        status = "Diabetic"
        if y_prob > 0.9 or hba1c > 8.0:
            risk_level = "High"
        elif y_prob > 0.7 or hba1c > 7.0:
            risk_level = "Medium"
        else:
            risk_level = "Low"
        
    elif prediction == 'P':
        # Pre-diabetic case with variable risk based on clinical factors
        binary_result = "Negative"
        status = "Pre-diabetic"
        
        # Assess risk based on clinical factors
        if hba1c >= 6.0 or bmi >= 30 or p_prob > 0.9:
            risk_level = "High"
        elif hba1c >= 5.8 or bmi >= 27 or p_prob > 0.7:
            risk_level = "Medium"
        else:
            risk_level = "Low"  # Changed from "Low-Medium" to simply "Low"
            
    else:  # 'N' case
        # Negative case with variable risk
        binary_result = "Negative"
        status = "Non-diabetic"
        
        # Risk assessment based on probability and clinical values
        if p_prob > 0.3 or hba1c >= 5.7:
            risk_level = "Medium"
        elif bmi > 27 or y_prob > 0.1:
            risk_level = "Low"
        else:
            risk_level = "Negative"  # Very low risk
    
    # Generate recommendation
    if binary_result == 'Positive':
        if risk_level == 'High':
            recommendation = "Immediate medical attention required. Comprehensive diabetes management needed."
        elif risk_level == 'Medium':
            recommendation = "Medical consultation required. Medication may be needed."
        else:
            recommendation = "Medical consultation recommended. Early diabetes management needed."
    elif status == 'Pre-diabetic':
        recommendation = "Immediate lifestyle intervention. Follow-up in 3 months."
    else:  # Negative
        if risk_level == 'Medium':
            recommendation = "Lifestyle changes recommended. Follow-up in 6 months."
        elif risk_level == 'Low':
            recommendation = "Minor lifestyle adjustments recommended. Follow-up in 1 year."
        else:
            recommendation = "Continue healthy lifestyle. Regular check-up in 1 year."
    
    # Return comprehensive assessment
    return {
        'original_class': prediction,
        'binary_result': binary_result,
        'risk_level': risk_level,
        'clinical_status': status,
        'probabilities': {
            'N': n_prob,
            'P': p_prob,
            'Y': y_prob
        },
        'recommendation': recommendation,
        'key_indicators': {
            'HbA1c': hba1c,
            'BMI': bmi
        },
        'request_data': patient_data
    }

# Helper function to get risk level for heart disease
def get_risk_level(probability: float) -> str:
    if probability < 0.50:
        return "LOW_RISK"
    elif probability < 0.75:
        return "MODERATE_RISK"
    elif probability < 0.90:
        return "HIGH_RISK"
    else:
        return "VERY_HIGH_RISK"

# API Endpoints
# Heart Disease Prediction - Both JSON and Form data endpoints
@app.post("/api/heart-disease-prediction", response_model=HeartDiseaseResponse, 
          responses={400: {"model": ErrorResponse}, 401: {"model": ErrorResponse}, 500: {"model": ErrorResponse}})
async def predict_heart_disease(request: dict, api_key: str = Depends(validate_api_key)):
    # Make sure model is loaded
    if models["heart"]["model"] is None or models["heart"]["preprocessor"] is None:
        if not load_model("heart"):
            raise HTTPException(
                status_code=500,
                detail={"error": "Failed to load heart disease model", "status": "error"}
            )
    
    try:
        # Log the complete request
        logger.info(f"Heart disease prediction request: {request}")
        
        # Define expected column names
        heart_columns = ['age', 'sex', 'cp', 'trestbps', 'chol', 'fbs', 'restecg', 'thalach', 
                         'exang', 'oldpeak', 'slope', 'ca', 'thal']
        values = None
        request_data = {}
        
        # Check if data is provided in the array format
        if "data" in request and isinstance(request["data"], list) and len(request["data"]) > 0:
            # Get the first row of data directly
            values = request["data"][0]  # Extract the inner array directly
            logger.info(f"Received heart disease data in array format: {values}")
            
            # Create request_data dictionary for response
            for i, col in enumerate(heart_columns):
                if i < len(values):
                    request_data[col] = values[i]
            
            # Validate array length
            if len(values) != len(heart_columns):
                raise HTTPException(
                    status_code=400,
                    detail={"error": f"Expected {len(heart_columns)} values in data array, got {len(values)}", "status": "error"}
                )
        else:
            # Fall back to the named fields approach
            logger.info("Received heart disease data in named fields format")
            try:
                # First try direct attribute access
                values = [
                    getattr(request, "age", None),
                    getattr(request, "sex", None),
                    getattr(request, "cp", None),
                    getattr(request, "trestbps", None),
                    getattr(request, "chol", None),
                    getattr(request, "fbs", None),
                    getattr(request, "restecg", None),
                    getattr(request, "thalach", None),
                    getattr(request, "exang", None),
                    getattr(request, "oldpeak", None),
                    getattr(request, "slope", None),
                    getattr(request, "ca", None),
                    getattr(request, "thal", None)
                ]
                # Create request_data dictionary for response
                for i, col in enumerate(heart_columns):
                    request_data[col] = values[i]
            except (AttributeError, TypeError):
                # Then try dictionary access
                values = [
                    request.get("age"),
                    request.get("sex"),
                    request.get("cp"),
                    request.get("trestbps"),
                    request.get("chol"),
                    request.get("fbs"),
                    request.get("restecg"),
                    request.get("thalach"),
                    request.get("exang"),
                    request.get("oldpeak"),
                    request.get("slope"),
                    request.get("ca"),
                    request.get("thal")
                ]
                # Create request_data dictionary for response
                for i, col in enumerate(heart_columns):
                    request_data[col] = values[i]
        
        # Check if any critical values are missing
        if None in values:
            missing_columns = [heart_columns[i] for i, v in enumerate(values) if v is None]
            raise HTTPException(
                status_code=400,
                detail={"error": f"Missing required parameters: {', '.join(missing_columns)}", "status": "error"}
            )
        
        # Create a pandas DataFrame with the expected column names
        input_df = pd.DataFrame([values], columns=heart_columns)
        logger.info(f"Created DataFrame for heart disease prediction: {input_df}")
        
        # Preprocess the data
        processed_data = models["heart"]["preprocessor"].transform(input_df)
        
        # Make prediction
        probability = models["heart"]["model"].predict_proba(processed_data)[0][1]
        prediction = "POSITIVE" if probability >= 0.5 else "NEGATIVE"
        
        logger.info(f"Heart disease prediction: {prediction}, probability: {probability}")
        
        # Include request data in response
        response = {
            "prediction": prediction,
            "probability": round(float(probability), 2),
            "status": "success",
            "request_data": request_data
        }
        
        logger.info(f"Heart disease response: {response}")
        return response
        
    except HTTPException:
        # Re-raise HTTP exceptions
        raise
    except Exception as e:
        logger.error(f"Heart disease prediction error: {e}")
        import traceback
        traceback.print_exc()
        raise HTTPException(
            status_code=500,
            detail={"error": f"Model prediction failed: {str(e)}", "status": "error"}
        )

# Form-based endpoint for Heart Disease
@app.post("/api/heart-disease-form", response_model=HeartDiseaseResponse)
async def predict_heart_disease_form(
    age: int = Form(...),
    sex: int = Form(...),
    cp: int = Form(...),
    trestbps: int = Form(...),
    chol: int = Form(...),
    fbs: int = Form(...),
    restecg: int = Form(...),
    thalach: int = Form(...),
    exang: int = Form(...),
    oldpeak: float = Form(...),
    slope: int = Form(...),
    ca: int = Form(...),
    thal: int = Form(...),
    api_key: str = Depends(validate_api_key)
):
    # Validate inputs according to requirements
    if age < 25 or age > 90:
        return JSONResponse(status_code=400, content={"error": "Age must be between 25 and 90", "status": "error"})
    if sex not in [0, 1]:
        return JSONResponse(status_code=400, content={"error": "Sex must be 0 (Female) or 1 (Male)", "status": "error"})
    if cp not in [0, 1, 2, 3]:
        return JSONResponse(status_code=400, content={"error": "Chest pain type must be between 0 and 3", "status": "error"})
    if trestbps < 90 or trestbps > 200:
        return JSONResponse(status_code=400, content={"error": "Resting blood pressure must be between 90 and 200 mm Hg", "status": "error"})
    if chol < 120 or chol > 570:
        return JSONResponse(status_code=400, content={"error": "Cholesterol must be between 120 and 570 mg/dl", "status": "error"})
    
    try:
        # Create a request dictionary with the data in the array format expected by the prediction endpoint
        request = {
            "data": [[
                age, sex, cp, trestbps, chol, fbs, restecg, thalach, exang, oldpeak, slope, ca, thal
            ]]
        }
        
        # Call the prediction endpoint
        return await predict_heart_disease(request, api_key)
    except Exception as e:
        logger.error(f"Heart disease form prediction error: {e}")
        import traceback
        traceback.print_exc()
        return JSONResponse(
            status_code=500,
            content={"error": f"Model prediction failed: {str(e)}", "status": "error"}
        )

# Diabetes Prediction - Both JSON and Form data endpoints
@app.post("/api/diabetes-prediction", response_model=DiabetesResponse,
          responses={400: {"model": ErrorResponse}, 401: {"model": ErrorResponse}, 500: {"model": ErrorResponse}})
async def predict_diabetes(request: dict, api_key: str = Depends(validate_api_key)):
    # Make sure model is loaded
    if models["diabetes"]["model"] is None or models["diabetes"]["scaler"] is None:
        if not load_model("diabetes"):
            raise HTTPException(
                status_code=500,
                detail={"error": "Failed to load diabetes model", "status": "error"}
            )
    
    try:
        # Log the complete request
        logger.info(f"Diabetes prediction request: {request}")
        
        # Define expected column names
        diabetes_columns = ['Gender', 'AGE', 'Urea', 'Cr', 'HbA1c', 'Chol', 'TG', 'HDL', 'LDL', 'VLDL', 'BMI']
        values = None
        request_data = {}
        
        # Check if data is provided in the array format
        if "data" in request and isinstance(request["data"], list) and len(request["data"]) > 0:
            # Use the first row of data
            values = request["data"][0]  # Extract the inner array directly
            logger.info(f"Received diabetes data in array format: {values}")
            
            # Create request_data dictionary for response
            for i, col in enumerate(diabetes_columns):
                if i < len(values):
                    request_data[col] = values[i]
            
            # Validate array length
            if len(values) != len(diabetes_columns):
                raise HTTPException(
                    status_code=400,
                    detail={"error": f"Expected {len(diabetes_columns)} values in data array, got {len(values)}", "status": "error"}
                )
        else:
            # Fall back to the named fields approach
            logger.info("Received diabetes data in named fields format")
            try:
                # First try direct attribute access
                values = [
                    getattr(request, "Gender", None),
                    getattr(request, "AGE", None),
                    getattr(request, "Urea", None),
                    getattr(request, "Cr", None),
                    getattr(request, "HbA1c", None),
                    getattr(request, "Chol", None),
                    getattr(request, "TG", None),
                    getattr(request, "HDL", None),
                    getattr(request, "LDL", None),
                    getattr(request, "VLDL", None),
                    getattr(request, "BMI", None)
                ]
                # Create request_data dictionary for response
                for i, col in enumerate(diabetes_columns):
                    request_data[col] = values[i]
            except (AttributeError, TypeError):
                # Then try dictionary access
                values = [
                    request.get("Gender"),
                    request.get("AGE"),
                    request.get("Urea"),
                    request.get("Cr"),
                    request.get("HbA1c"),
                    request.get("Chol"),
                    request.get("TG"),
                    request.get("HDL"),
                    request.get("LDL"),
                    request.get("VLDL"),
                    request.get("BMI")
                ]
                # Create request_data dictionary for response
                for i, col in enumerate(diabetes_columns):
                    request_data[col] = values[i]
        
        # Create a patient data dictionary
        patient_data = dict(zip(diabetes_columns, values))
        logger.info(f"Created patient data for diabetes prediction: {patient_data}")
        
        # Check for missing critical values
        if patient_data['HbA1c'] is None or patient_data['BMI'] is None:
            raise HTTPException(
                status_code=400,
                detail={"error": "Missing required parameters: HbA1c and BMI must be provided", "status": "error"}
            )
            
        # If any values are None, use default safe values (this is optional)
        for key in patient_data:
            if patient_data[key] is None:
                if key in ['Gender', 'AGE', 'Chol', 'TG', 'HDL', 'LDL']:
                    patient_data[key] = 0  # Default integer
                else:
                    patient_data[key] = 0.0  # Default float
                logger.warning(f"Using default value 0 for missing parameter: {key}")
        
        # Use the comprehensive risk assessment function
        risk_assessment = predict_diabetes_with_risk(
            patient_data, 
            model=models["diabetes"]["model"], 
            scaler=models["diabetes"]["scaler"]
        )
        
        # Add request_data and status field required by API response
        risk_assessment['request_data'] = request_data
        risk_assessment['status'] = 'success'
        
        logger.info(f"Diabetes risk assessment complete: {risk_assessment}")
        
        return risk_assessment
        
    except HTTPException:
        # Re-raise HTTP exceptions
        raise
    except Exception as e:
        logger.error(f"Diabetes prediction error: {e}")
        import traceback
        traceback.print_exc()
        raise HTTPException(
            status_code=500,
            detail={"error": f"Model prediction failed: {str(e)}", "status": "error"}
        )

# Form-based endpoint for Diabetes
@app.post("/api/diabetes-form", response_model=DiabetesResponse)
async def predict_diabetes_form(
    Gender: int = Form(...),
    AGE: int = Form(...),
    Urea: float = Form(...),
    Cr: float = Form(...),
    HbA1c: float = Form(...),
    Chol: int = Form(...),
    TG: int = Form(...),
    HDL: int = Form(...),
    LDL: int = Form(...),
    VLDL: float = Form(...),
    BMI: float = Form(...),
    api_key: str = Depends(validate_api_key)
):
    # Perform input validation
    if Gender not in [0, 1]:
        return JSONResponse(status_code=400, content={"error": "Gender must be 0 (Male) or 1 (Female)", "status": "error"})
    if AGE <= 0 or AGE >= 120:
        return JSONResponse(status_code=400, content={"error": "Age must be a positive value less than 120", "status": "error"})
    if HbA1c <= 0 or HbA1c >= 15:
        return JSONResponse(status_code=400, content={"error": "HbA1c must be a positive value less than 15%", "status": "error"})
    
    try:
        # Create a request dictionary with the data in the array format expected by the prediction endpoint
        request = {
            "data": [[
                Gender, AGE, Urea, Cr, HbA1c, Chol, TG, HDL, LDL, VLDL, BMI
            ]]
        }
        
        # Call the prediction endpoint
        return await predict_diabetes(request, api_key)
    except Exception as e:
        logger.error(f"Diabetes form prediction error: {e}")
        import traceback
        traceback.print_exc()
        return JSONResponse(
            status_code=500,
            content={"error": f"Model prediction failed: {str(e)}", "status": "error"}
        )

# Symptom endpoints
@app.get("/api/symptoms", responses={401: {"model": ErrorResponse}, 500: {"model": ErrorResponse}})
async def get_symptoms(api_key: str = Depends(validate_api_key)):
    # Make sure model components are loaded
    if models["symptom"]["all_symptoms"] is None:
        if not load_model("symptom"):
            raise HTTPException(
                status_code=500,
                detail={"error": "Failed to load symptom checker components", "status": "error"}
            )
    
    # Return the list of symptoms
    all_symptoms = models["symptom"]["all_symptoms"]
    symptom_list = all_symptoms.tolist() if isinstance(all_symptoms, np.ndarray) else all_symptoms
    
    # Log available symptoms for debugging
    logger.info(f"Available symptoms: {symptom_list[:20]}... (total: {len(symptom_list)})")
    
    return {"symptoms": symptom_list}

@app.post("/api/predict", response_model=SymptomResponse, 
         responses={400: {"model": ErrorResponse}, 401: {"model": ErrorResponse}, 500: {"model": ErrorResponse}})
async def predict_disease(request: SymptomRequest, api_key: str = Depends(validate_api_key)):
    # Make sure model components are loaded
    if any(models["symptom"][key] is None for key in ["model", "encoder", "all_symptoms", "descriptions", "precautions"]):
        if not load_model("symptom"):
            raise HTTPException(
                status_code=500,
                detail={"error": "Failed to load symptom checker components", "status": "error"}
            )
    
    try:
        # Log the complete request
        logger.info(f"Symptom prediction request: {request.dict()}")
        
        # Normalize symptoms input - replace hyphens AND spaces with underscores
        symptoms_input = [symptom.replace('-', '_').replace(' ', '_') for symptom in request.symptoms]
        logger.info(f"Original symptoms: {request.symptoms}")
        logger.info(f"Normalized symptoms: {symptoms_input}")
        
        top_n = request.top_n
        all_symptoms = models["symptom"]["all_symptoms"]
        
        # Validate minimum symptoms
        if not symptoms_input or len(symptoms_input) < 1:
            raise HTTPException(
                status_code=400,
                detail={"error": "At least one symptom is required", "status": "error"}
            )
        
        # Convert all_symptoms to list if it's numpy array
        all_symptoms_list = all_symptoms.tolist() if isinstance(all_symptoms, np.ndarray) else all_symptoms
        
        # Convert all_symptoms to have underscores for comparison
        all_symptoms_list_normalized = [s.replace(' ', '_').replace('-', '_') for s in all_symptoms_list]
        
        # Log available symptoms for debugging
        logger.info(f"Sample of available symptoms: {all_symptoms_list[:5]}")
        
        # Validate symptoms
        invalid_symptoms = [s for s in symptoms_input if s not in all_symptoms_list_normalized]
        if invalid_symptoms:
            # Try to find potential matches
            suggestions = {}
            for invalid in invalid_symptoms:
                for valid, normalized in zip(all_symptoms_list, all_symptoms_list_normalized):
                    if invalid in normalized or normalized in invalid:
                        suggestions[invalid] = valid
                        
            error_msg = f"Invalid symptoms: {', '.join(invalid_symptoms)}"
            if suggestions:
                error_msg += f". Did you mean: {', '.join([f'{k} → {v}' for k, v in suggestions.items()])}"
                
            raise HTTPException(
                status_code=400,
                detail={"error": error_msg, "status": "error"}
            )
        
        # Convert symptoms to feature vector
        features = np.zeros(len(all_symptoms_list))
        
        # Set 1s for symptoms that are present
        for symptom in symptoms_input:
            # Find the index in the normalized list
            symptom_index = all_symptoms_list_normalized.index(symptom)
            features[symptom_index] = 1
        
        # Reshape features for prediction
        features = features.reshape(1, -1)
        
        # Get prediction probabilities
        prediction_probabilities = models["symptom"]["model"].predict_proba(features)[0]
        
        # Get indices of top N predictions
        top_indices = prediction_probabilities.argsort()[-top_n:][::-1]
        
        # Get disease names and confidence scores
        predictions = []
        for idx in top_indices:
            disease = models["symptom"]["encoder"].inverse_transform([idx])[0]
            confidence = prediction_probabilities[idx]
            
            # Get disease description
            description = "No description available."
            precautions = []
            
            # Find description for the disease
            disease_desc_df = models["symptom"]["descriptions"]
            disease_desc_row = disease_desc_df[disease_desc_df['Disease'] == disease]
            if not disease_desc_row.empty:
                description = disease_desc_row['Description'].values[0]
            
            # Find precautions for the disease
            disease_prec_df = models["symptom"]["precautions"]
            disease_prec_row = disease_prec_df[disease_prec_df['Disease'] == disease]
            if not disease_prec_row.empty:
                for i in range(1, 5):
                    precaution = disease_prec_row[f'Precaution_{i}'].values[0]
                    if isinstance(precaution, str) and precaution.strip():
                        precautions.append(precaution)
            
            predictions.append({
                "disease": disease,
                "probability": round(float(confidence), 2),
                "description": description,
                "precautions": precautions
            })
        
        # Create the response with request data
        response = {
            "predictions": predictions,
            "status": "success",
            "request_data": {
                "symptoms": request.symptoms,
                "top_n": request.top_n
            }
        }
        
        logger.info(f"Symptom prediction response: {response}")
        return response
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Disease prediction error: {e}")
        import traceback
        traceback.print_exc()
        raise HTTPException(
            status_code=500,
            detail={"error": str(e), "status": "error"}
        )

# Form-based endpoint for Symptom Checker
@app.post("/api/predict-form", response_model=SymptomResponse)
async def predict_disease_form(
    symptoms: str = Form(...),
    top_n: int = Form(3),
    api_key: str = Depends(validate_api_key)
):
    # Convert comma-separated symptoms to list and clean them
    symptoms_list = [s.strip() for s in symptoms.split(',') if s.strip()]
    
    # Normalize symptoms by replacing hyphens with underscores
    symptoms_list = [symptom.replace('-', '_').replace(' ', '_') for symptom in symptoms_list]
    
    logger.info(f"Form symptoms after normalization: {symptoms_list}")
    
    # Validate minimum symptoms
    if not symptoms_list:
        return JSONResponse(
            status_code=400, 
            content={"error": "At least one symptom is required", "status": "error"}
        )
        
    # Create a request object to reuse the existing endpoint logic
    request = SymptomRequest(symptoms=symptoms_list, top_n=top_n)
    
    # Call the JSON endpoint handler with this request
    return await predict_disease(request, api_key)

@app.get("/")
async def root():
    # Check which models are loaded
    heart_status = "loaded" if models["heart"]["model"] is not None else "not loaded"
    diabetes_status = "loaded" if models["diabetes"]["model"] is not None else "not loaded"
    symptom_status = "loaded" if models["symptom"]["model"] is not None else "not loaded"
    
    return {
        "message": "Disease Detection API is running. See /docs for API documentation.",
        "models_status": {
            "heart_disease": heart_status,
            "diabetes": diabetes_status,
            "symptom_checker": symptom_status
        }
    }

# Add a middleware to log all requests
@app.middleware("http")
async def log_requests(request: Request, call_next):
    request_body = b""
    try:
        request_body = await request.body()
    except Exception:
        pass
    
    # Log the request details
    logger.info(f"Request: {request.method} {request.url.path}")
    if request_body:
        try:
            body_str = request_body.decode()
            logger.info(f"Request body: {body_str}")
        except:
            logger.info(f"Request body: [binary data]")
    
    # Process the request and get the response
    response = await call_next(request)
    
    # Return the response
    return response

# Run with: uvicorn ApiModels:app --reload
if __name__ == "__main__":
    import uvicorn
    uvicorn.run("ApiModels:app", host="0.0.0.0", port=8000, reload=True)