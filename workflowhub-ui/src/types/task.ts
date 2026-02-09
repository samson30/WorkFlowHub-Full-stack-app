// Task status values matching backend enum
export type TaskStatus = "Todo" | "InProgress" | "Done";

export interface Task {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  createdAt: string;
  projectId: number;
}

// Column configuration for Kanban board
export const COLUMNS: { id: TaskStatus; title: string }[] = [
  { id: "Todo", title: "To Do" },
  { id: "InProgress", title: "In Progress" },
  { id: "Done", title: "Done" },
];
