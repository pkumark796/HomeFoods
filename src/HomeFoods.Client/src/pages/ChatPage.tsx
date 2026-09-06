import { useState } from 'react';
import { Box, Paper, TextField, IconButton, Button, List, ListItem, ListItemText, Avatar, Typography } from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import { chatService } from '../services/api';

type Message = { sender: 'user' | 'assistant'; text: string };

export default function ChatPage() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const start = async () => {
    const res = await chatService.startChat();
    setSessionId(res.sessionId);
    setMessages([{ sender: 'assistant', text: 'Hello! How can I help you today?' }]);
  };

  const send = async () => {
    if (!input || loading) return;
    if (!sessionId) {
      await start();
    }

    const text = input;
    setInput('');
    setMessages(prev => [...prev, { sender: 'user', text }] );
    setLoading(true);

    try {
      const res = await chatService.sendMessage(sessionId ?? '', text);
      setMessages(prev => [...prev, { sender: 'assistant', text: res.reply }]);
    } catch (err) {
      setMessages(prev => [...prev, { sender: 'assistant', text: 'Sorry, something went wrong.' }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
      <Paper sx={{ width: '100%', maxWidth: 900, p: 2 }} elevation={3}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>Assistant Chat</Typography>
          <Button variant="outlined" onClick={start}>Start Chat</Button>
        </Box>

        <List sx={{ maxHeight: '60vh', overflow: 'auto', mb: 2 }}>
          {messages.map((m, i) => (
            <ListItem key={i} sx={{ alignItems: 'flex-start' }}>
              <Avatar sx={{ mr: 2 }}>{m.sender === 'user' ? 'U' : 'A'}</Avatar>
              <ListItemText primary={m.text} secondary={m.sender === 'user' ? 'You' : 'Assistant'} />
            </ListItem>
          ))}
        </List>

        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            fullWidth
            placeholder="Type a message..."
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => { if (e.key === 'Enter') send(); }}
          />
          <IconButton color="primary" onClick={send} disabled={loading} aria-label="send">
            <SendIcon />
          </IconButton>
        </Box>
      </Paper>
    </Box>
  );
}
