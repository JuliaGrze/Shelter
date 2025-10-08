export interface LoginDto{
    email: string
    password: string
}

export interface RegisterDto{
    email: string
    password: string
    firstName?: string | null
    lastName?: string | null
}

export interface AuthResponse {
    token: string;
    userId: string;
    email: string;
    roles: string[];
    firstName?: string | null;
    lastName?: string | null;
}

export interface UserProfile{
    userId: string;
    email: string;
    roles: string[];
    firstName?: string | null;
    lastName?: string | null;
}