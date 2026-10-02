export enum ProjectStatus {
  Planning = 1,
  Active = 2,
  OnHold = 3,
  Completed = 4,
  Archived = 5
}

export interface ProjectDto {
  id: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  startDate?: string;
  targetEndDate?: string;
  taskCount: number;
  createdBy?: string;
  createdAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description?: string;
  startDate?: string;
  targetEndDate?: string;
}