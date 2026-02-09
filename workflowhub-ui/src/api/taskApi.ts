import api from "./api";
import { Task, TaskStatus } from "../types/task";

export const getTasks = async (projectId: number): Promise<Task[]> => {
  const res = await api.get(`/projects/${projectId}/tasks`);
  return res.data;
};

export const createTask = async (
  projectId: number,
  data: { title: string; description: string; status: TaskStatus }
): Promise<Task> => {
  const res = await api.post(`/projects/${projectId}/tasks`, data);
  return res.data;
};

export const updateTaskStatus = async (
  projectId: number,
  taskId: number,
  status: TaskStatus
): Promise<Task> => {
const res = await api.patch(`/projects/${projectId}/tasks/${taskId}/status`, { status });
  return res.data;
};

export const updateTask = async (
  projectId: number,
  taskId: number,
  data: { title: string; description: string; status: TaskStatus }
): Promise<void> => {
  await api.put(`/projects/${projectId}/tasks/${taskId}`, data);
};

export const deleteTask = async (projectId: number, taskId: number): Promise<void> => {
  await api.delete(`/projects/${projectId}/tasks/${taskId}`);
};
