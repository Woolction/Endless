import { api } from "./axios";

export const login = async (email, password) => {
    const res = await api.post("/Auth/token", { email, password });
    return res.data;
};

export const register = async (data) => {
    const res = await api.post("/Users", data);
    return res.data;
};

export const logout = async () => {
    await api.delete("/Auth/token");
    localStorage.removeItem("token");
};