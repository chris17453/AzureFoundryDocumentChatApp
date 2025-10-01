import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Chip,
  Stack,
  IconButton,
  Box,
  Grid,
} from '@mui/material';
import { Delete, Chat, Description } from '@mui/icons-material';
import { Document } from '../api/client';

interface DocumentListProps {
  documents: Document[];
  onChatWithDocument: (document: Document) => void;
  onDeleteDocument: (documentId: string) => void;
}

export const DocumentList: React.FC<DocumentListProps> = ({
  documents,
  onChatWithDocument,
  onDeleteDocument,
}) => {
  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <Grid container spacing={2}>
      {documents.map((document) => (
        <Grid item xs={12} sm={6} md={4} key={document.id}>
          <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <CardContent sx={{ flexGrow: 1 }}>
              <Stack spacing={2}>
                <Box display="flex" alignItems="center" gap={1}>
                  <Description color="primary" />
                  <Typography variant="h6" component="div" noWrap>
                    {document.fileName}
                  </Typography>
                </Box>

                <Stack direction="row" spacing={1} flexWrap="wrap">
                  <Chip
                    label={`${document.pageCount} pages`}
                    size="small"
                    variant="outlined"
                  />
                  <Chip
                    label={`${document.wordCount} words`}
                    size="small"
                    variant="outlined"
                  />
                  <Chip
                    label={formatFileSize(document.fileSizeBytes)}
                    size="small"
                    variant="outlined"
                  />
                </Stack>

                {document.contentPreview && (
                  <Typography variant="body2" color="text.secondary" noWrap>
                    {document.contentPreview}
                  </Typography>
                )}

                <Typography variant="caption" color="text.secondary">
                  Uploaded: {formatDate(document.uploadedAt)}
                </Typography>
              </Stack>
            </CardContent>

            <Box sx={{ p: 1, pt: 0 }}>
              <Stack direction="row" justifyContent="space-between">
                <IconButton
                  color="primary"
                  onClick={() => onChatWithDocument(document)}
                  size="small"
                >
                  <Chat />
                </IconButton>
                <IconButton
                  color="error"
                  onClick={() => onDeleteDocument(document.id)}
                  size="small"
                >
                  <Delete />
                </IconButton>
              </Stack>
            </Box>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};