import client  from "../../shared/api/client";
import type { AuthResponse, LoginRequest, RegisterRequest } from "./types";

export const login = (data: LoginRequest) => 
    client.post<AuthResponse>('/auth/login', data);

export const register = (data: RegisterRequest) =>
    client.post<AuthResponse>('/auth/register', data);