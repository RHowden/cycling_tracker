import { createContext, useContext, useEffect, useState } from "react";
import { apiRequest } from "../api/client";
import { UserInfo, UserInfoSchema } from "../api/schemas"

type AuthState = "loading" | "authenticated" | "unauthenticated";

const AuthContext = createContext<{ status: AuthState; refresh: () => void }>({
    status: "loading",
    refresh: () => { },
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [status, setStatus] = useState<AuthState>("loading");
    const [_, setUser] = useState<UserInfo | null>(null);

    const checkAuth = async () => {
        try {
            const info = await apiRequest("/auth/manage/info", UserInfoSchema);
            setUser(info);
            setStatus("authenticated");
        } catch {
            setUser(null);
            setStatus("unauthenticated");
        }
    };

    useEffect(() => { checkAuth(); }, []);

    return (
        <AuthContext.Provider value={{ status, refresh: checkAuth }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);