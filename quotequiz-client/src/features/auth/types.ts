 export interface AuthResponse {
    token: string;
    username: string;
    email: string;
    role: string;
}

  export interface LoginRequest {
    emailOrUsername: string;
    password: string;
}

  export interface RegisterRequest {
    userName: string;
    email: string;
    password: string;
}