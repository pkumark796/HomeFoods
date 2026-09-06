import type { ReactNode } from 'react';
import { Box, Typography, Button } from '@mui/material';
import { Inbox as InboxIcon, ShoppingCart as CartIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  message: string;
  actionLabel?: string;
  onAction?: () => void;
}

export function EmptyState({
  icon,
  title,
  message,
  actionLabel,
  onAction,
}: EmptyStateProps) {
  const navigate = useNavigate();

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '400px',
        textAlign: 'center',
        px: 3,
      }}
    >
      <Box
        sx={{
          width: 80,
          height: 80,
          borderRadius: '50%',
          backgroundColor: 'background.default',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
        }}
      >
        {icon || <InboxIcon sx={{ fontSize: 48, color: 'text.secondary' }} />}
      </Box>
      <Typography variant="h5" gutterBottom fontWeight={600}>
        {title}
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3, maxWidth: 400 }}>
        {message}
      </Typography>
      {actionLabel && onAction && (
        <Button variant="contained" size="large" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </Box>
  );
}

export function EmptyCart() {
  const navigate = useNavigate();

  return (
    <EmptyState
      icon={<CartIcon sx={{ fontSize: 48, color: 'text.secondary' }} />}
      title="Your cart is empty"
      message="Looks like you haven't added any items to your cart yet. Start shopping to fill it up!"
      actionLabel="Start Shopping"
      onAction={() => navigate('/catalog')}
    />
  );
}

export default EmptyState;
