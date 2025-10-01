import React, { useState, useEffect } from 'react';
import {
  ThemeProvider,
  createTheme,
  CssBaseline,
  AppBar,
  Toolbar,
  Typography,
  Container,
  Grid,
  Tab,
  Tabs,
  Box,
  Alert,
  Snackbar,
} from '@mui/material';
import { DocumentUpload } from './components/DocumentUpload';
import { DocumentList } from './components/DocumentList';
import { ChatInterface } from './components/ChatInterface';
import { Document, ChatSession, documentsApi, chatApi } from './api/client';

// TODO: move theme to separate file later
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    // secondary: {
    //   main: '#dc004e', // keeping this for now, might use later
    // },
  },
});

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

function App() {
  const [tabValue, setTabValue] = useState(0);
  const [documents, setDocuments] = useState<Document[]>([]);
  const [chatSessions, setChatSessions] = useState<ChatSession[]>([]);
  const [currentSession, setCurrentSession] = useState<ChatSession | null>(null);
  const [loading, setLoading] = useState(true);
  const [notification, setNotification] = useState<string | null>(null);

  // Load initial data - probably should add error handling here at some point
  useEffect(() => {
    loadDocuments();
    loadChatSessions();
  }, []);

  const loadDocuments = async () => {
    try {
      const response = await documentsApi.getAll();
      setDocuments(response.data);
    } catch (error) {
      console.error('Failed to load documents:', error);
      // setNotification('Failed to load documents'); // uncomment when ready
    } finally {
      setLoading(false);
    }
  };

  const loadChatSessions = async () => {
    try {
      const response = await chatApi.getSessions();
      setChatSessions(response.data);
    } catch (error) {
      console.error('Failed to load chat sessions:', error);
    }
  };

  const handleDocumentUpload = (document: Document) => {
    setDocuments(prev => [document, ...prev]);
    setNotification(`Document "${document.fileName}" uploaded successfully!`);
    // Auto-switch to documents tab to show the new upload
    setTabValue(1);
  };

  const handleChatWithDocument = async (document: Document) => {
    try {
      // Create new chat session for this document
      const sessionTitle = `Chat with ${document.fileName}`;
      const response = await chatApi.createSession(sessionTitle, document.id);
      
      setCurrentSession(response.data);
      setChatSessions(prev => [response.data, ...prev]);
      setTabValue(2); // Switch to chat tab
    } catch (error) {
      console.error('Failed to create chat session:', error);
      setNotification('Failed to start chat session');
    }
  };

  const handleDeleteDocument = async (documentId: string) => {
    try {
      await documentsApi.delete(documentId);
      setDocuments(prev => prev.filter(doc => doc.id !== documentId));
      setNotification('Document deleted');
    } catch (error) {
      console.error('Failed to delete document:', error);
      setNotification('Failed to delete document');
    }
  };

  const handleSessionUpdate = (session: ChatSession) => {
    setCurrentSession(session);
    // Update the session in the list too
    setChatSessions(prev => 
      prev.map(s => s.id === session.id ? session : s)
    );
  };

  /*
   * Note: Consider adding global search functionality later
   * Maybe add document preview modal too?
   * Also need to handle connection errors better
   */

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Azure AI Document Chat
            {/* <span style={{fontSize: '0.7em', opacity: 0.7}}> v0.1.0-beta</span> */}
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ mt: 2 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)}>
            <Tab label="Upload" />
            <Tab label={`Documents (${documents.length})`} />
            <Tab label="Chat" disabled={!currentSession} />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <DocumentUpload onUploadSuccess={handleDocumentUpload} />
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          {documents.length === 0 ? (
            <Alert severity="info">
              No documents uploaded yet. Go to the Upload tab to get started.
            </Alert>
          ) : (
            <DocumentList
              documents={documents}
              onChatWithDocument={handleChatWithDocument}
              onDeleteDocument={handleDeleteDocument}
            />
          )}
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          {currentSession ? (
            <Box sx={{ height: 'calc(100vh - 200px)' }}>
              <ChatInterface
                session={currentSession}
                onSessionUpdate={handleSessionUpdate}
              />
            </Box>
          ) : (
            <Alert severity="info">
              Select a document to start chatting with it.
            </Alert>
          )}
        </TabPanel>
      </Container>

      {/* Simple notification system - replace with proper toast later */}
      <Snackbar
        open={!!notification}
        autoHideDuration={4000}
        onClose={() => setNotification(null)}
        message={notification}
      />
    </ThemeProvider>
  );
}

export default App;
