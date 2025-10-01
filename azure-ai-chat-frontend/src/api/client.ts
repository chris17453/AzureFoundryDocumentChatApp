import axios from 'axios';

const API_BASE_URL = 'https://localhost:7067/api';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface Document {
  id: string;
  fileName: string;
  uploadedAt: string;
  contentType: string;
  fileSizeBytes: number;
  pageCount: number;
  wordCount: number;
  content?: string;
  contentPreview?: string;
}

export interface ChatSession {
  id: string;
  title: string;
  createdAt: string;
  lastUpdatedAt: string;
  document?: {
    id: string;
    fileName: string;
  };
  messages?: ChatMessage[];
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  sourceDocuments?: string;
}

export const documentsApi = {
  upload: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post<Document>('/documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },
  
  getAll: () => apiClient.get<Document[]>('/documents'),
  
  getById: (id: string) => apiClient.get<Document>(`/documents/${id}`),
  
  search: (query: string, maxResults = 5) => 
    apiClient.get<Document[]>(`/documents/search?query=${encodeURIComponent(query)}&maxResults=${maxResults}`),
  
  delete: (id: string) => apiClient.delete(`/documents/${id}`),
};

export const chatApi = {
  createSession: (title: string, documentId?: string) =>
    apiClient.post<ChatSession>('/chat/sessions', { title, documentId }),
  
  getSessions: () => apiClient.get<ChatSession[]>('/chat/sessions'),
  
  getSession: (sessionId: string) => apiClient.get<ChatSession>(`/chat/sessions/${sessionId}`),
  
  sendMessage: (sessionId: string, content: string) =>
    apiClient.post<ChatMessage>(`/chat/sessions/${sessionId}/messages`, { content }),
};