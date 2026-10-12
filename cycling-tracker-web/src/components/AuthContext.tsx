import { createContext, useCallback, useEffect, useState, useContext } from "react";
import { apiRequest, apiRequestVoid, setUnauthorizedHandler } from "../api/client";
import { UserInfoSchema } from "../api/schemas";
import { login as loginRequest } from "../api/auth";

type AuthState = "loading" | "authenticated" | "unauthenticated";

const AuthContext = createContext<{
    status: AuthState;
    refresh: () => Promise<void>;
    logout: () => Promise<void>;
    login: (email: string, password: string) => Promise<void>;
}>({ status: "loading", refresh: async () => { }, logout: async () => { },
    login: async () => {} });

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [status, setStatus] = useState<AuthState>("loading");

    const refresh = useCallback(async () => {
        try {
            await apiRequest("/auth/manage/info", UserInfoSchema);
            setStatus("authenticated");
        } catch {
            setStatus("unauthenticated");
        }
    }, []);

    const logout = useCallback(async () => {
        try {
            await apiRequestVoid("/auth/logout", { method: "POST" });
        } finally {
            setStatus("unauthenticated"); // even if the call fails, stop trusting the UI state
        }
    }, []);

    const login = useCallback(async (email: string, password: string) => {
        await loginRequest(email, password);
        setStatus("authenticated");
    }, []);

    useEffect(() => {
        refresh();
        setUnauthorizedHandler(() => setStatus("unauthenticated"));

        // Back/forward cache: the page is restored with stale state and no remount
        const onPageShow = (e: PageTransitionEvent) => { if (e.persisted) refresh(); };
        window.addEventListener("pageshow", onPageShow);

        return () => {
            setUnauthorizedHandler(null);
            window.removeEventListener("pageshow", onPageShow);
        };
    }, [refresh]);

    return (
        <AuthContext.Provider value={{ status, refresh, logout, login }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);