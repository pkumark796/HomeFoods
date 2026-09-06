import { useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  TextField,
  Divider,
  Card,
  CardContent,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  ShoppingCart as ShoppingCartIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { useCart } from '../context/CartContext';

function CartPage() {
  const navigate = useNavigate();
  const { cartItems, removeFromCart, updateQuantity, getTotalPrice, clearCart } = useCart();

  const subtotal = getTotalPrice();
  const tax = subtotal * 0.08; // 8% tax
  const deliveryFee = subtotal > 50 ? 0 : 5.99;
  const total = subtotal + tax + deliveryFee;

  if (cartItems.length === 0) {
    return (
      <Container maxWidth="lg" sx={{ py: 8, textAlign: 'center' }}>
        <ShoppingCartIcon sx={{ fontSize: 100, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h5" gutterBottom>
          Your cart is empty
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          Start shopping to add items to your cart
        </Typography>
        <Button
          variant="contained"
          size="large"
          onClick={() => navigate('/catalog')}
        >
          Continue Shopping
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/catalog')}
        sx={{ mb: 3 }}
      >
        Continue Shopping
      </Button>

      <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" sx={{ mb: 4 }}>
        Shopping Cart ({cartItems.length} items)
      </Typography>

      <Box sx={{ display: 'flex', gap: 3, flexDirection: { xs: 'column', md: 'row' } }}>
        {/* Cart Items */}
        <Box sx={{ flex: 1 }}>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Product</TableCell>
                  <TableCell align="right">Price</TableCell>
                  <TableCell align="center">Quantity</TableCell>
                  <TableCell align="right">Total</TableCell>
                  <TableCell align="center">Remove</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {cartItems.map((item) => {
                  const price = item.product.discountedPrice || item.product.price;
                  const itemTotal = price * item.quantity;

                  return (
                    <TableRow key={item.product.id}>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                          <img
                            src={item.product.imageUrl || 'https://via.placeholder.com/80'}
                            alt={item.product.name}
                            style={{ width: 80, height: 80, objectFit: 'cover', borderRadius: 4 }}
                          />
                          <Box>
                            <Typography variant="subtitle1">{item.product.name}</Typography>
                            <Typography variant="body2" color="text.secondary">
                              {item.product.brand?.name ?? 'Unknown brand'}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {item.product.weight} {item.product.unit}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell align="right">
                        <Typography variant="body1">${price.toFixed(2)}</Typography>
                      </TableCell>
                      <TableCell align="center">
                        <TextField
                          type="number"
                          value={item.quantity}
                          onChange={(e) => updateQuantity(item.product.id, parseInt(e.target.value))}
                          inputProps={{ min: 1, max: item.product.stockQuantity }}
                          size="small"
                          sx={{ width: 80 }}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Typography variant="body1" fontWeight="bold">
                          ${itemTotal.toFixed(2)}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <IconButton
                          color="error"
                          onClick={() => removeFromCart(item.product.id)}
                        >
                          <DeleteIcon />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ mt: 2, textAlign: 'right' }}>
            <Button
              variant="outlined"
              color="error"
              onClick={clearCart}
            >
              Clear Cart
            </Button>
          </Box>
        </Box>

        {/* Order Summary */}
        <Box sx={{ width: { xs: '100%', md: 350 } }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom fontWeight="bold">
                Order Summary
              </Typography>
              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant="body2">Subtotal:</Typography>
                <Typography variant="body2">${subtotal.toFixed(2)}</Typography>
              </Box>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                <Typography variant="body2">Tax (8%):</Typography>
                <Typography variant="body2">${tax.toFixed(2)}</Typography>
              </Box>

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="body2">Delivery Fee:</Typography>
                <Typography variant="body2">
                  {deliveryFee === 0 ? 'FREE' : `$${deliveryFee.toFixed(2)}`}
                </Typography>
              </Box>

              {subtotal < 50 && (
                <Typography variant="caption" color="success.main" sx={{ mb: 2, display: 'block' }}>
                  Add ${(50 - subtotal).toFixed(2)} more to get free delivery!
                </Typography>
              )}

              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                <Typography variant="h6" fontWeight="bold">Total:</Typography>
                <Typography variant="h6" fontWeight="bold" color="primary">
                  ${total.toFixed(2)}
                </Typography>
              </Box>

              <Button
                fullWidth
                variant="contained"
                size="large"
                onClick={() => navigate('/checkout')}
                sx={{
                  fontWeight: 700,
                  py: 1.5,
                  background: 'linear-gradient(135deg, #FF6F00 0%, #FF9800 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #F57C00 0%, #FB8C00 100%)',
                    transform: 'translateY(-2px)',
                    boxShadow: 4
                  }
                }}
              >
                Proceed to Checkout
              </Button>
            </CardContent>
          </Card>
        </Box>
      </Box>
    </Container>
  );
}

export default CartPage;
