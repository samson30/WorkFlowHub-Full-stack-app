import api from "./api";
import { Project } from "../types/project";

export const getProjects = async (): Promise<Project[]> => {
  const res = await api.get("/projects");
  return res.data;
};

export const getProjectById = async (projectId: number): Promise<Project> => {
  const res = await api.get(`/projects/${projectId}`);
  return res.data;
};

export const createProject = async (data: { name: string; description: string }): Promise<Project> => {
  const res = await api.post("/projects", {
    name: data.name,
    description: data.description,
    status: "Active"
  });
  return res.data;
};

export const updateProject = async (
  projectId: number,
  data: { name: string; description: string; status: string }
): Promise<void> => {
  await api.put(`/projects/${projectId}`, data);
};

export const deleteProject = async (projectId: number): Promise<void> => {
  await api.delete(`/projects/${projectId}`);
};
