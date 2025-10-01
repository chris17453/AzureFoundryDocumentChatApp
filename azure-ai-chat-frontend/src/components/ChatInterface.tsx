import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Paper,
  TextField,
  IconButton,
  Typography,
  Stack,
  Avatar,
  Chip,
  Divider,
} from '@mui/material';
import { Send, Person, SmartToy, Description } from '@mui/icons-material';
import { ChatSession, ChatMessage, chatApi } from '../api/client';

interface ChatInterfaceProps {
  session: ChatSession;
  onSessionUpdate: (session: ChatSession) => void;
}

export const ChatInterface: React.FC<ChatInterfaceProps> = ({
  session,
  onSessionUpdate,
}) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [sending, setSending] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    loadSession();
  }, [session.id]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const loadSession = async () => {
    try {
      const response = await chatApi.getSession(session.id);
      setMessages(response.data.messages || []);
      onSessionUpdate(response.data);
    } catch (error) {
      console.error('Failed to load session:', error);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async () => {
    if (!newMessage.trim() || sending) return;

    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      role: 'user',
      content: newMessage,
      timestamp: new Date().toISOString(),
    };

    setMessages(prev => [...prev, userMessage]);
    setNewMessage('');
    setSending(true);

    try {
      const response = await chatApi.sendMessage(session.id, newMessage);
      setMessages(prev => [...prev, response.data]);
    } catch (error) {
      console.error('Failed to send message:', error);
    } finally {
      setSending(false);
    }
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      handleSendMessage();
    }
  };

  const formatTime = (timestamp: string) => {
    return new Date(timestamp).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const parseSourceDocuments = (sourceDocuments?: string) => {
    try {
      return sourceDocuments ? JSON.parse(sourceDocuments) : [];
    } catch {
      return [];
    }
  };

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Stack direction="row" alignItems="center" spacing={2}>
          <SmartToy color="primary" />
          <Box>
            <Typography variant="h6">{session.title}</Typography>
            {session.document && (
              <Stack direction="row" alignItems="center" spacing={1}>
                <Description fontSize="small" />
                <Typography variant="body2" color="text.secondary">
                  {session.document.fileName}
                </Typography>
              </Stack>
            )}
          </Box>
        </Stack>
      </Paper>

      {/* Messages */}
      <Box sx={{ flex: 1, overflow: 'auto', p: 1 }}>
        <Stack spacing={2}>
          {messages.map((message) => (
            <Box
              key={message.id}
              sx={{
                display: 'flex',
                justifyContent: message.role === 'user' ? 'flex-end' : 'flex-start',
              }}
            >
              <Box
                sx={{
                  maxWidth: '70%',
                  display: 'flex',
                  flexDirection: message.role === 'user' ? 'row-reverse' : 'row',
                  alignItems: 'flex-start',
                  gap: 1,
                }}
              >
                <Avatar
                  sx={{
                    bgcolor: message.role === 'user' ? 'primary.main' : 'secondary.main',
                    width: 32,
                    height: 32,
                  }}
                >
                  {message.role === 'user' ? <Person /> : <SmartToy />}
                </Avatar>

                <Paper
                  sx={{
                    p: 2,
                    bgcolor: message.role === 'user' ? 'primary.main' : 'grey.100',
                    color: message.role === 'user' ? 'primary.contrastText' : 'text.primary',
                  }}
                >
                  <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                    {message.content}
                  </Typography>

                  {message.sourceDocuments && (
                    <Box sx={{ mt: 1, pt: 1, borderTop: '1px solid rgba(0,0,0,0.1)' }}>
                      <Typography variant="caption" display="block" gutterBottom>
                        Sources:
                      </Typography>
                      <Stack direction="row" spacing={1} flexWrap="wrap">
                        {parseSourceDocuments(message.sourceDocuments).map((doc: any) => (
                          <Chip
                            key={doc.Id}
                            label={doc.FileName}
                            size="small"
                            variant="outlined"
                          />
                        ))}
                      </Stack>
                    </Box>
                  )}

                  <Typography
                    variant="caption"
                    sx={{
                      display: 'block',
                      textAlign: 'right',
                      mt: 1,
                      opacity: 0.7,
                    }}
                  >
                    {formatTime(message.timestamp)}
                  </Typography>
                </Paper>
              </Box>
            </Box>
          ))}
          <div ref={messagesEndRef} />
        </Stack>
      </Box>

      {/* Input */}
      <Paper sx={{ p: 2, mt: 2 }}>
        <Stack direction="row" spacing={1} alignItems="flex-end">
          <TextField
            fullWidth
            multiline
            maxRows={4}
            placeholder="Ask a question about the document..."
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onKeyPress={handleKeyPress}
            disabled={sending}
          />
          <IconButton
            color="primary"
            onClick={handleSendMessage}
            disabled={!newMessage.trim() || sending}
          >
            <Send />
          </IconButton>
        </Stack>
      </Paper>
    </Box>
  );
};