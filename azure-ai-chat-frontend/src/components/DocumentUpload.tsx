import React, { useState, useCallback } from 'react';
import {
  Box,
  Button,
  Paper,
  Typography,
  LinearProgress,
  Alert,
  Stack,
  Chip,
} from '@mui/material';
import { CloudUpload, Description } from '@mui/icons-material';
import { documentsApi, Document } from '../api/client';

interface DocumentUploadProps {
  onUploadSuccess: (document: Document) => void;
}

export const DocumentUpload: React.FC<DocumentUploadProps> = ({ onUploadSuccess }) => {
  const [uploading, setUploading] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFileUpload = useCallback(async (file: File) => {
    if (!file) return;

    // TODO: add file size validation (maybe 50MB limit?)
    // if (file.size > 50 * 1024 * 1024) {
    //   setError('File too large. Please select a file under 50MB.');
    //   return;
    // }

    setUploading(true);
    setError(null);

    try {
      const response = await documentsApi.upload(file);
      onUploadSuccess(response.data);
    } catch (err: any) {
      // This error handling could be better - might want to show specific error types
      setError(err.response?.data || 'Failed to upload document');
      console.log('Upload error:', err); // keeping this for debugging
    } finally {
      setUploading(false);
    }
  }, [onUploadSuccess]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    
    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0) {
      handleFileUpload(files[0]);
    }
  }, [handleFileUpload]);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
  }, []);

  const handleFileSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files && files.length > 0) {
      handleFileUpload(files[0]);
    }
  }, [handleFileUpload]);

  return (
    <Paper
      sx={{
        p: 4,
        textAlign: 'center',
        border: '2px dashed',
        borderColor: dragOver ? 'primary.main' : 'grey.300',
        backgroundColor: dragOver ? 'action.hover' : 'background.paper',
        transition: 'all 0.2s ease-in-out',
      }}
      onDrop={handleDrop}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
    >
      <Stack spacing={3} alignItems="center">
        <CloudUpload sx={{ fontSize: 64, color: 'primary.main' }} />
        
        <Box>
          <Typography variant="h6" gutterBottom>
            Upload Document
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Drag and drop a document here, or click to select
          </Typography>
        </Box>

        <Stack direction="row" spacing={1}>
          <Chip label="PDF" size="small" />
          <Chip label="DOCX" size="small" />
          <Chip label="TXT" size="small" />
          <Chip label="Images" size="small" />
        </Stack>

        <input
          type="file"
          accept=".pdf,.docx,.doc,.txt,.png,.jpg,.jpeg"
          style={{ display: 'none' }}
          id="file-upload"
          onChange={handleFileSelect}
          disabled={uploading}
        />
        <label htmlFor="file-upload">
          <Button
            variant="contained"
            component="span"
            startIcon={<Description />}
            disabled={uploading}
          >
            Choose File
          </Button>
        </label>

        {uploading && (
          <Box sx={{ width: '100%', maxWidth: 300 }}>
            <LinearProgress />
            <Typography variant="body2" sx={{ mt: 1 }}>
              Processing document...
            </Typography>
          </Box>
        )}

        {error && (
          <Alert severity="error" sx={{ width: '100%' }}>
            {error}
          </Alert>
        )}
      </Stack>
    </Paper>
  );
};