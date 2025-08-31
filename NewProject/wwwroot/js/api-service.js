/**
 * Disease Detection API Service
 * This file provides JavaScript functions to connect to the disease detection API
 */

// API configuration (modify this based on your deployment)
const API_CONFIG = {
    // Use the global settings if available, otherwise use default
    baseUrl: (window.API_SETTINGS && window.API_SETTINGS.baseUrl) 
        ? `${window.API_SETTINGS.baseUrl}/api` 
        : 'http://localhost:8000/api',
    timeout: 30000 // Request timeout in milliseconds
};

// Log the API configuration for debugging
console.log('Disease Detection API configured with base URL:', API_CONFIG.baseUrl);

/**
 * Heart Disease Prediction API Client
 */
const HeartDiseaseService = {
    /**
     * Predict heart disease based on input parameters
     * @param {Object} params - The form parameters
     * @returns {Promise} - Promise resolving to prediction results
     */
    predict: function(params) {
        return new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                reject(new Error('Request timed out'));
            }, API_CONFIG.timeout);

            // Format data in the exact format the backend expects - a nested array
            const requestData = {
                data: [[
                    params.age,
                    params.sex,
                    params.cp,
                    params.trestbps,
                    params.chol,
                    params.fbs,
                    params.restecg,
                    params.thalach,
                    params.exang,
                    params.oldpeak,
                    params.slope,
                    params.ca,
                    params.thal
                ]]
            };

            console.log('Heart disease request data:', JSON.stringify(requestData));

            fetch(`${API_CONFIG.baseUrl}/heart-disease-prediction`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            })
            .then(response => {
                clearTimeout(timeoutId);
                if (!response.ok) {
                    throw new Error(`Server responded with ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                // Format prediction message
                if (!data.message) {
                    if (data.prediction === 'POSITIVE') {
                        data.message = 'The model indicates a significant risk of heart disease. Please consult with a healthcare professional for a proper diagnosis.';
                    } else {
                        data.message = 'The model indicates a low risk of heart disease. Continue maintaining a healthy lifestyle and regular check-ups.';
                    }
                }
                
                // Format probability as percentage
                if (typeof data.probability === 'number') {
                    data.probabilityFormatted = `${(data.probability * 100).toFixed(2)}%`;
                }
                
                resolve(data);
            })
            .catch(error => {
                clearTimeout(timeoutId);
                console.error('Heart Disease API Error:', error);
                reject(error);
            });
        });
    }
};

/**
 * Diabetes Prediction API Client
 */
const DiabetesService = {
    /**
     * Predict diabetes based on input parameters
     * @param {Object} params - The form parameters
     * @returns {Promise} - Promise resolving to prediction results
     */
    predict: function(params) {
        return new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                reject(new Error('Request timed out'));
            }, API_CONFIG.timeout);

            // Format data in the exact format the backend expects - a nested array
            // Note the capitalization of parameter names to match the backend expectations
            const requestData = {
                data: [[
                    params.Gender,
                    params.AGE,
                    params.Urea,
                    params.Cr,
                    params.HbA1c,
                    params.Chol,
                    params.TG,
                    params.HDL,
                    params.LDL,
                    params.VLDL,
                    params.BMI
                ]]
            };

            console.log('Diabetes request data:', JSON.stringify(requestData));

            fetch(`${API_CONFIG.baseUrl}/diabetes-prediction`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestData)
            })
            .then(response => {
                clearTimeout(timeoutId);
                if (!response.ok) {
                    throw new Error(`Server responded with ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                resolve(data);
            })
            .catch(error => {
                clearTimeout(timeoutId);
                console.error('Diabetes API Error:', error);
                reject(error);
            });
        });
    }
};

/**
 * Health metrics utilities
 */
const HealthMetrics = {
    /**
     * Get HbA1c status classification
     * @param {number} value - HbA1c value
     * @returns {string} - Status classification
     */
    getHbA1cStatus: function(value) {
        if (value < 5.7) return 'Normal';
        if (value < 6.5) return 'Prediabetic';
        return 'Diabetic';
    },
    
    /**
     * Get BMI status classification
     * @param {number} value - BMI value
     * @returns {string} - Status classification
     */
    getBMIStatus: function(value) {
        if (value < 18.5) return 'Underweight';
        if (value < 25) return 'Normal';
        if (value < 30) return 'Overweight';
        return 'Obese';
    },
    
    /**
     * Get cholesterol status classification
     * @param {number} value - Cholesterol value in mg/dl
     * @returns {string} - Status classification
     */
    getCholesterolStatus: function(value) {
        if (value < 200) return 'Normal';
        if (value < 240) return 'Borderline High';
        return 'High';
    }
};

/**
 * Form utilities
 */
const FormUtils = {
    /**
     * Convert form to JavaScript object
     * @param {HTMLFormElement} form - The form element
     * @returns {Object} - Form data as object
     */
    formToObject: function(form) {
        const formData = new FormData(form);
        const formObject = {};
        
        formData.forEach((value, key) => {
            // Convert numeric values to numbers
            const numValue = Number(value);
            formObject[key] = !isNaN(numValue) ? numValue : value;
        });
        
        return formObject;
    }
}; 