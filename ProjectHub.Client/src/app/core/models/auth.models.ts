export interface AuthResponse {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  companyId: string;
  accessToken: string;
  refreshToken: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  companyId: string;
  roleId: string;
}

export interface UserSession {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  companyId: string;
}