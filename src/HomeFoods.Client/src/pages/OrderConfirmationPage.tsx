import { useEffect, useState } from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  CircularProgress,
  Alert,
} from '@mui/material';
import { CheckCircle as CheckCircleIcon } from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';

function OrderConfirmationPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // In a real app, you might want to verify the payment status here
    setLoading(false);
  }, [orderId]);

  if (loading) {
    return (
      <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Verifying your order...</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm" sx={{ py: 8 }}>
      <Paper sx={{ p: 4, textAlign: 'center' }}>
        <CheckCircleIcon sx={{ fontSize: 80, color: 'success.main', mb: 2 }} />

        <Typography variant="h4" gutterBottom fontWeight={600}>
          Order Placed Successfully!
        </Typography>

        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          Thank you for your order. We'll send you a confirmation email shortly.
        </Typography>

        <Alert severity="success" sx={{ mb: 3 }}>
          <Typography variant="body2">
            Order ID: <strong>{orderId}</strong>
          </Typography>
        </Alert>

        <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
          Your groceries will be delivered within 2-3 business days.
        </Typography>

        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
          <Button
            variant="contained"
            color="primary"
            onClick={() => navigate('/catalog')}
          >
            Continue Shopping
          </Button>
          <Button
            variant="outlined"
            color="primary"
            onClick={() => navigate('/')}
          >
            Go to Home
          </Button>
        </Box>
      </Paper>
    </Container>
  );
}

export default OrderConfirmationPage;
