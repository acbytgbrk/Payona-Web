/**
 * Authentication Module - Handles login, register, and auth state
 */

import { apiClient } from './api.js';

class AuthService {
  async register(email, password, name, surname) {
    const response = await apiClient.post('/auth/register', {
      email,
      password,
      name,
      surname,
    });
    
    // Backend returns token and user, save them
    if (response.token || response.Token) {
      const token = response.token || response.Token;
      const user = response.user || response.User;
      apiClient.setToken(token);
      if (user) {
        sessionStorage.setItem('user', JSON.stringify(user));
      }
    }
    
    return response;
  }

  async login(email, password) {
    const response = await apiClient.post('/auth/login', {
      email,
      password,
    });
    
    if (response.token) {
      apiClient.setToken(response.token);
      // Store user info
      sessionStorage.setItem('user', JSON.stringify(response.user));
    }
    
    return response;
  }

  async updateDormInfo(userId, gender, city, dorm) {
    const response = await apiClient.put(`/auth/dorm-info/${userId}`, {
      gender,
      city,
      dorm,
    });
    return response;
  }

  async updateProfile(name, surname, email) {
    const response = await apiClient.put('/auth/profile', {
      name,
      surname,
      email,
    });
    
    if (response.user) {
      sessionStorage.setItem('user', JSON.stringify(response.user));
    }
    
    return response;
  }

  async changePassword(oldPassword, newPassword) {
    return apiClient.post('/auth/change-password', {
      oldPassword,
      newPassword,
    });
  }

  async deleteAccount(userId, password) {
    return apiClient.delete(`/auth/delete-account/${userId}`, {
      password,
    });
  }

  async logout() {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      apiClient.setToken('');
      sessionStorage.removeItem('user');
      window.location.href = '/';
    }
  }

  isAuthenticated() {
    return !!apiClient.getToken();
  }

  getCurrentUser() {
    const userStr = sessionStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  requiresDormInfo() {
    const user = this.getCurrentUser();
    return !user || !user.gender || !user.city || !user.dorm;
  }
}

export const authService = new AuthService();

