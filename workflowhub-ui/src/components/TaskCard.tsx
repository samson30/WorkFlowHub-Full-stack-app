import { useDraggable } from "@dnd-kit/core";
import { Task } from "../types/task";

type Props = {
  task: Task;
  isDragging?: boolean;
  onDelete?: (taskId: number) => void;
  onEdit?: (task: Task) => void;
};

export default function TaskCard({ task, isDragging, onDelete, onEdit }: Props) {
  const { attributes, listeners, setNodeRef, transform } = useDraggable({
    id: task.id,
  });

  const style = transform
    ? {
        transform: `translate3d(${transform.x}px, ${transform.y}px, 0)`,
      }
    : undefined;

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      className={`bg-white p-3 rounded-lg shadow-sm border border-gray-200 cursor-grab active:cursor-grabbing ${
        isDragging ? "opacity-50 shadow-lg" : ""
      }`}
    >
      <div className="flex justify-between items-start">
        <h4 className="font-medium text-gray-800">{task.title}</h4>
        <div className="flex gap-1 ml-2">
          {onEdit && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onEdit(task);
              }}
              className="text-gray-400 hover:text-blue-600 text-sm"
            >
              Edit
            </button>
          )}
          {onDelete && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onDelete(task.id);
              }}
              className="text-red-400 hover:text-red-600 text-sm"
            >
              ✕
            </button>
          )}
        </div>
      </div>
      {task.description && (
        <p className="text-gray-500 text-sm mt-1 line-clamp-2">
          {task.description}
        </p>
      )}
    </div>
  );
}
