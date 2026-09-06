import { useState } from 'react';
import { Drawer, IconButton, Box, Tooltip } from '@mui/material';
import ChatBubbleIcon from '@mui/icons-material/ChatBubble';
import CloseIcon from '@mui/icons-material/Close';
import ChatPage from '../pages/ChatPage';

export default function ChatWidget() {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Tooltip title={open ? 'Close chat' : 'Open chat'}>
        <IconButton
          color="primary"
          onClick={() => setOpen(true)}
          sx={{
            position: 'fixed',
            right: 24,
            bottom: 24,
            zIndex: (theme) => theme.zIndex.drawer + 2,
            bgcolor: 'background.paper',
            boxShadow: 3,
            '&:hover': { bgcolor: 'background.paper' },
          }}
          aria-label="open chat"
        >
          <ChatBubbleIcon />
        </IconButton>
      </Tooltip>

      <Drawer
        anchor="right"
        open={open}
        onClose={() => setOpen(false)}
        PaperProps={{ sx: { width: { xs: '100%', sm: 420 }, maxWidth: 480 } }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', p: 1 }}>
          <IconButton onClick={() => setOpen(false)} aria-label="close chat">
            <CloseIcon />
          </IconButton>
        </Box>
        <Box sx={{ height: 'calc(100% - 56px)', overflow: 'auto' }}>
          <ChatPage />
        </Box>
      </Drawer>
    </>
  );
}
