import { useEffect, useState, type ChangeEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  Grid,
  Card,
  CardMedia,
  Chip,
  TextField,
  CircularProgress,
  Divider,
} from '@mui/material';
import {
  AddShoppingCart as AddShoppingCartIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { Product } from '../types';
import { productService } from '../services/api';
import { useCart } from '../context/CartContext';

function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToCart } = useCart();
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);

  useEffect(() => {
    const loadProduct = async () => {
      if (!id) return;

      try {
        setLoading(true);
        const data = await productService.getById(id);
        setProduct(data);
      } catch (error) {
        console.error('Error loading product:', error);
      } finally {
        setLoading(false);
      }
    };

    loadProduct();
  }, [id]);

  const handleAddToCart = () => {
    if (product) {
      addToCart(product, quantity);
      // Show a success message or redirect to cart
      navigate('/cart');
    }
  };

  const handleQuantityChange = (event: ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    if (value > 0 && value <= (product?.stockQuantity || 0)) {
      setQuantity(value);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!product) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Typography variant="h5">Product not found</Typography>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/catalog')} sx={{ mt: 2 }}>
          Back to Catalog
        </Button>
      </Container>
    );
  }

  const price = product.discountedPrice || product.price;
  const hasDiscount = product.discountedPrice && product.discountedPrice < product.price;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/catalog')}
        sx={{ mb: 3 }}
      >
        Back to Catalog
      </Button>

      <Grid container spacing={4}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardMedia
              component="img"
              image={product.imageUrl || 'https://via.placeholder.com/600x400?text=No+Image'}
              alt={product.name}
              sx={{ width: '100%', height: 'auto', maxHeight: 500, objectFit: 'contain' }}
            />
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
            {product.name}
          </Typography>

          <Typography variant="subtitle1" color="text.secondary" sx={{ mb: 2 }}>
            Brand: {product.brand?.name ?? 'Unknown'}
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            <Typography variant="h4" color="primary" fontWeight="bold">
              ${price.toFixed(2)}
            </Typography>
            {hasDiscount && (
              <>
                <Typography
                  variant="h6"
                  sx={{ textDecoration: 'line-through', color: 'text.secondary' }}
                >
                  ${product.price.toFixed(2)}
                </Typography>
                <Chip
                  label={`${Math.round(((product.price - product.discountedPrice!) / product.price) * 100)}% OFF`}
                  color="error"
                />
              </>
            )}
          </Box>

          <Box sx={{ mb: 3 }}>
            <Chip
              label={product.category?.name ?? 'Uncategorized'}
              color="primary"
              variant="outlined"
              sx={{ mr: 1 }}
            />
            {product.stockQuantity === 0 ? (
              <Chip label="Out of Stock" color="error" />
            ) : (
              <Chip label={`${product.stockQuantity} in stock`} color="success" />
            )}
          </Box>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Description
          </Typography>
          <Typography variant="body1" color="text.secondary" paragraph>
            {product.description}
          </Typography>

          <Box sx={{ mb: 3 }}>
            <Typography variant="body2" color="text.secondary">
              <strong>Weight:</strong> {product.weight} {product.unit}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              <strong>SKU:</strong> {product.sku}
            </Typography>
            {product.origin && (
              <Typography variant="body2" color="text.secondary">
                <strong>Origin:</strong> {product.origin}
              </Typography>
            )}
            {product.expiryDate && (
              <Typography variant="body2" color="text.secondary">
                <strong>Expiry Date:</strong> {new Date(product.expiryDate).toLocaleDateString()}
              </Typography>
            )}
          </Box>

          <Divider sx={{ my: 3 }} />

          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            <TextField
              type="number"
              label="Quantity"
              value={quantity}
              onChange={handleQuantityChange}
              inputProps={{ min: 1, max: product.stockQuantity }}
              sx={{ width: 100 }}
            />
            <Button
              variant="contained"
              size="large"
              startIcon={<AddShoppingCartIcon />}
              onClick={handleAddToCart}
              disabled={product.stockQuantity === 0}
              sx={{ flexGrow: 1 }}
            >
              Add to Cart
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Container>
  );
}

export default ProductDetailPage;
