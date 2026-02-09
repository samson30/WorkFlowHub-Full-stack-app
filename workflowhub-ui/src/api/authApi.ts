import api from "./api";

export const login = async (email: string, password: string) => {
  const res = await api.post("/auth/login", { email, password });
  return res.data.token; // Return token instead of saving it
};

export const register = async (email: string, password: string) => {
  await api.post("/auth/register", { email, password });
};
