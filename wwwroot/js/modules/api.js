/**
 * API Module - Handles all HTTP requests to the backend
 */

const API_BASE_URL = '/api';

class ApiClient {
  constructor() {
    this.token = this.getToken();
  }

  getToken() {
    // Try to get token from httpOnly cookie first, then fallback to sessionStorage
    // Since we can't access httpOnly cookies from JS, we'll use sessionStorage
    // In production, consider using httpOnly cookies set by the server
    return sessionStorage.getItem('authToken') || '';
  }

  setToken(token) {
    this.token = token;
    if (token) {
      sessionStorage.setItem('authToken', token);
    } else {
      sessionStorage.removeItem('authToken');
    }
  }

  async request(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const config = {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    };

    if (this.token) {
      config.headers['Authorization'] = `Bearer ${this.token}`;
    }

    try {
      const response = await fetch(url, config);
      const data = await response.json();

      if (!response.ok) {
        // Try to get a user-friendly error message
        let errorMessage = 'An error occurred';
        
        if (data.message) {
          errorMessage = data.message;
        } else if (data.errors) {
          // Handle validation errors
          const errorMessages = [];
          for (const key in data.errors) {
            if (Array.isArray(data.errors[key])) {
              errorMessages.push(...data.errors[key]);
            } else {
              errorMessages.push(data.errors[key]);
            }
          }
          errorMessage = errorMessages.join(' ');
        } else if (data.title) {
          errorMessage = data.title;
        } else if (typeof data === 'string') {
          errorMessage = data;
        }
        
        throw new Error(errorMessage);
      }

      return data;
    } catch (error) {
      console.error('API Error:', error);
      // If it's already an Error with a message, re-throw it
      if (error instanceof Error) {
        throw error;
      }
      // Otherwise wrap it
      throw new Error(error.message || 'An error occurred');
    }
  }

  async get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  }

  async post(endpoint, data) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async put(endpoint, data) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async delete(endpoint, data = null) {
    const options = { method: 'DELETE' };
    if (data) {
      options.body = JSON.stringify(data);
    }
    return this.request(endpoint, options);
  }
}

export const apiClient = new ApiClient();

