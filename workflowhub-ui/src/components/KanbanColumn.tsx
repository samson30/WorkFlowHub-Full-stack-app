import { useDroppable } from "@dnd-kit/core";
import { Task, TaskStatus } from "../types/task";
import TaskCard from "./TaskCard";

type Props = {
  id: TaskStatus;
  title: string;
  tasks: Task[];
  onDelete: (taskId: number) => void;
  onEdit: (task: Task) => void;
};

export default function KanbanColumn({ id, title, tasks, onDelete, onEdit }: Props) {
  const { setNodeRef, isOver } = useDroppable({ id });

  const columnColors: Record<TaskStatus, string> = {
    Todo: "bg-gray-100 border-gray-300",
    InProgress: "bg-blue-50 border-blue-300",
    Done: "bg-green-50 border-green-300",
  };

  const headerColors: Record<TaskStatus, string> = {
    Todo: "bg-gray-200 text-gray-700",
    InProgress: "bg-blue-200 text-blue-700",
    Done: "bg-green-200 text-green-700",
  };

  return (
    <div
      ref={setNodeRef}
      className={`flex-1 min-w-[280px] max-w-[350px] rounded-lg border-2 ${columnColors[id]} ${
        isOver ? "ring-2 ring-blue-400" : ""
      }`}
    >
      <div className={`p-3 rounded-t-lg font-semibold ${headerColors[id]}`}>
        {title}
        <span className="ml-2 text-sm font-normal">({tasks.length})</span>
      </div>

      <div className="p-2 space-y-2 min-h-[400px]">
        {tasks.map((task) => (
          <TaskCard key={task.id} task={task} onDelete={onDelete} onEdit={onEdit} />
        ))}

        {tasks.length === 0 && (
          <p className="text-gray-400 text-center py-4 text-sm">
            Drop tasks here
          </p>
        )}
      </div>
    </div>
  );
}
