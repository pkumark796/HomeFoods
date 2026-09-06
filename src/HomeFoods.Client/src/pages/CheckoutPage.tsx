import { useState, type ChangeEvent, type FormEvent } from 'react';
import {
  Container,
  Paper,
  Typography,
  Grid,
  TextField,
  Button,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormControl,
  FormLabel,
  Box,
  Divider,
  Alert,
  CircularProgress,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { checkoutService } from '../services/api';

interface CheckoutFormData {
  fullName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  paymentMethod: string;
  specialInstructions: string;
}

function CheckoutPage() {
  const navigate = useNavigate();
  const { cartItems, getTotalPrice, clearCart } = useCart();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState<CheckoutFormData>({
    fullName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    state: '',
    zipCode: '',
    paymentMethod: '3', // PhonePe
    specialInstructions: '',
  });

  const subTotal = getTotalPrice();
  const deliveryFee = subTotal < 500 ? 50 : 0;
  const tax = Math.round(subTotal * 0.05 * 100) / 100;
  const total = subTotal + deliveryFee + tax;

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Mock customer and address IDs (in production, these would come from authenticated user)
      const customerId = '00000000-0000-0000-0000-000000000001';
      const addressId = '00000000-0000-0000-0000-000000000001';

      const checkoutData = {
        customerId,
        deliveryAddressId: addressId,
        paymentMethod: parseInt(formData.paymentMethod),
        specialInstructions: formData.specialInstructions || null,
        items: cartItems.map((item) => ({
          productId: item.product.id,
          quantity: item.quantity,
        })),
      };

      const result = await checkoutService.checkout(checkoutData);

      if (result.payment?.phonePeUrl) {
        // Redirect to PhonePe payment page
        window.location.href = result.payment.phonePeUrl;
      } else {
        // Cash on delivery - go to confirmation
        clearCart();
        navigate(`/order-confirmation/${result.orderId}`);
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Checkout failed. Please try again.');
      setLoading(false);
    }
  };

  if (cartItems.length === 0) {
    return (
      <Container maxWidth="md" sx={{ py: 8 }}>
        <Alert severity="info">
          Your cart is empty. <Button onClick={() => navigate('/catalog')}>Continue Shopping</Button>
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom fontWeight={600} sx={{ mb: 4 }}>
        Checkout
      </Typography>

      <form onSubmit={handleSubmit}>
        <Grid container spacing={4}>
          {/* Left Column - Delivery & Payment Info */}
          <Grid item xs={12} md={8}>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Delivery Information
              </Typography>
              <Grid container spacing={2} sx={{ mt: 1 }}>
                <Grid item xs={12} sm={6}>
                  <TextField
                    required
                    fullWidth
                    label="Full Name"
                    name="fullName"
                    value={formData.fullName}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    required
                    fullWidth
                    label="Phone Number"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    label="Email"
                    name="email"
                    type="email"
                    value={formData.email}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    label="Address"
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    multiline
                    rows={2}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    required
                    fullWidth
                    label="City"
                    name="city"
                    value={formData.city}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    required
                    fullWidth
                    label="State"
                    name="state"
                    value={formData.state}
                    onChange={handleInputChange}
                  />
                </Grid>
                <Grid item xs={12} sm={4}>
                  <TextField
                    required
                    fullWidth
                    label="ZIP Code"
                    name="zipCode"
                    value={formData.zipCode}
                    onChange={handleInputChange}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <FormControl component="fieldset">
                <FormLabel component="legend" sx={{ fontWeight: 600, mb: 2 }}>
                  Payment Method
                </FormLabel>
                <RadioGroup
                  name="paymentMethod"
                  value={formData.paymentMethod}
                  onChange={handleInputChange}
                >
                  <FormControlLabel
                    value="3"
                    control={<Radio color="primary" />}
                    label={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography>PhonePe</Typography>
                        <Typography variant="caption" color="secondary">
                          (Recommended)
                        </Typography>
                      </Box>
                    }
                  />
                  <FormControlLabel
                    value="4"
                    control={<Radio color="primary" />}
                    label="Cash on Delivery"
                  />
                  <FormControlLabel
                    value="1"
                    control={<Radio color="primary" />}
                    label="Credit/Debit Card"
                    disabled
                  />
                </RadioGroup>
              </FormControl>
            </Paper>

            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Special Instructions (Optional)
              </Typography>
              <TextField
                fullWidth
                multiline
                rows={3}
                name="specialInstructions"
                placeholder="Any special delivery instructions..."
                value={formData.specialInstructions}
                onChange={handleInputChange}
              />
            </Paper>
          </Grid>

          {/* Right Column - Order Summary */}
          <Grid item xs={12} md={4}>
            <Paper sx={{ p: 3, position: 'sticky', top: 80 }}>
              <Typography variant="h6" gutterBottom fontWeight={600}>
                Order Summary
              </Typography>
              <Divider sx={{ my: 2 }} />

              {cartItems.map((item) => {
                const price = item.product.discountedPrice || item.product.price;
                return (
                  <Box key={item.product.id} sx={{ mb: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2">
                        {item.product.name} × {item.quantity}
                      </Typography>
                      <Typography variant="body2" fontWeight={500}>
                        ${(price * item.quantity).toFixed(2)}
                      </Typography>
                    </Box>
                  </Box>
                );
              })}

              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant="body2">Subtotal</Typography>
                <Typography variant="body2">${subTotal.toFixed(2)}</Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant="body2">Delivery Fee</Typography>
                <Typography variant="body2" color={deliveryFee === 0 ? 'success.main' : 'inherit'}>
                  {deliveryFee === 0 ? 'FREE' : `$${deliveryFee.toFixed(2)}`}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="body2">Tax (5% GST)</Typography>
                <Typography variant="body2">${tax.toFixed(2)}</Typography>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                <Typography variant="h6" fontWeight={600}>
                  Total
                </Typography>
                <Typography variant="h6" fontWeight={600} color="primary">
                  ${total.toFixed(2)}
                </Typography>
              </Box>

              {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                  {error}
                </Alert>
              )}

              <Button
                type="submit"
                variant="contained"
                fullWidth
                size="large"
                disabled={loading}
                startIcon={loading ? <CircularProgress size={20} color="inherit" /> : null}
                sx={{
                  fontWeight: 700,
                  py: 1.5,
                  background: 'linear-gradient(135deg, #FF6F00 0%, #FF9800 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #F57C00 0%, #FB8C00 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: 4
                  },
                  '&:disabled': {
                    background: 'rgba(0, 0, 0, 0.12)',
                  }
                }}
              >
                {loading ? 'Processing...' : 'Place Order'}
              </Button>

              <Button
                variant="outlined"
                fullWidth
                size="large"
                onClick={() => navigate('/cart')}
                sx={{ mt: 2 }}
              >
                Back to Cart
              </Button>
            </Paper>
          </Grid>
        </Grid>
      </form>
    </Container>
  );
}

export default CheckoutPage;
