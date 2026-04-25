export type UserDto = {
    id:string
    email:string
    userName:string
    displayName:string | null
    avatarUrl:string | null
    role: "User" | "Admin"
}

export type AuthResponse = {
    accessToken:string
    refreshToken:string
    user:UserDto
}

export type LoginRequest = {
    email:string
    password:string
}

export type RegisterRequest = {
    email:string
    userName:string
    password:string
}