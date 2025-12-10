/**
 * Authentication Module - FIXED VERSION
 */

import { apiClient } from './api.js';

class AuthService {

  normalizeUser(raw) {
    if (!raw) return null;

    return {
      id: raw.id ?? raw.Id,
      email: raw.email ?? raw.Email,
      name: raw.name ?? raw.Name,
      surname: raw.surname ?? raw.Surname,
      gender: raw.gender ?? raw.Gender,
      city: raw.city ?? raw.City,
      dorm: raw.dorm ?? raw.Dorm
    };
  }

  saveUser(rawUser) {
    const user = this.normalizeUser(rawUser);
    if (user) {
      sessionStorage.setItem('user', JSON.stringify(user));
    }
    return user;
  }

  async register(email, password, name, surname) {
    const response = await apiClient.post('/auth/register', {
      email,
      password,
      name,
      surname,
    });

    const token = response.token ?? response.Token;
    const rawUser = response.user ?? response.User;

    if (token) apiClient.setToken(token);
    if (rawUser) this.saveUser(rawUser);

    return response;
  }

  async login(email, password) {
    const response = await apiClient.post('/auth/login', {
      email,
      password,
    });

    const token = response.token ?? response.Token;
    const rawUser = response.user ?? response.User;

    if (token) apiClient.setToken(token);
    if (rawUser) this.saveUser(rawUser);

    return response;
  }

  async updateDormInfo(userId, gender, city, dorm) {
    const response = await apiClient.put(`/auth/dorm-info/${userId}`, {
      gender,
      city,
      dorm,
    });

    if (response.user) this.saveUser(response.user);

    return response;
  }

  async updateProfile(name, surname, email) {
    const response = await apiClient.put('/auth/profile', {
      name,
      surname,
      email,
    });

    if (response.user) this.saveUser(response.user);

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