import { useState, type MouseEvent } from 'react';
import { Card, CardMedia, CardContent, CardActions, Typography, Button, Box, Chip, IconButton } from '@mui/material';
import { AddShoppingCart as AddShoppingCartIcon, Add as AddIcon, Remove as RemoveIcon, Favorite as FavoriteIcon, FavoriteBorder as FavoriteBorderIcon } from '@mui/icons-material';
import { Product } from '../types';
import { useCart } from '../context/CartContext';
import { useNavigate } from 'react-router-dom';

interface ProductCardProps {
  product: Product;
}

function ProductCard({ product }: ProductCardProps) {
  const { addToCart, cartItems, updateQuantity } = useCart();
  const navigate = useNavigate();
  const [isFavorite, setIsFavorite] = useState(false);
  const [imageError, setImageError] = useState(false);

  const cartItem = cartItems.find(item => item.product.id === product.id);
  const quantity = cartItem?.quantity || 0;

  const handleAddToCart = (e: MouseEvent) => {
    e.stopPropagation();
    addToCart(product);
  };

  const handleQuantityChange = (e: MouseEvent, delta: number) => {
    e.stopPropagation();
    if (delta > 0) {
      addToCart(product);
    } else if (quantity > 0) {
      updateQuantity(product.id, quantity - 1);
    }
  };

  const toggleFavorite = (e: MouseEvent) => {
    e.stopPropagation();
    setIsFavorite(!isFavorite);
  };

  const price = product.discountedPrice || product.price;
  const hasDiscount = product.discountedPrice && product.discountedPrice < product.price;
  const discountPercentage = product.discountPercentage || (hasDiscount 
    ? Math.round(((product.price - product.discountedPrice!) / product.price) * 100)
    : 0);
  const isOutOfStock = product.stockQuantity === 0;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity <= 5;

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        cursor: 'pointer',
        transition: 'all 0.3s ease',
        position: 'relative',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: '0 8px 24px rgba(132, 194, 37, 0.15)',
        },
      }}
      onClick={() => navigate(`/product/${product.id}`)}
    >
      {/* Badges */}
      <Box sx={{ position: 'absolute', top: 12, left: 12, zIndex: 1, display: 'flex', flexDirection: 'column', gap: 0.5 }}>
        {hasDiscount && discountPercentage > 0 && (
          <Chip
            label={`${discountPercentage}% OFF`}
            size="small"
            sx={{
              bgcolor: 'secondary.main',
              color: '#000',
              fontWeight: 700,
              fontSize: '0.75rem',
            }}
          />
        )}
        {product.isFeatured && (
          <Chip
            label="Featured"
            size="small"
            sx={{
              bgcolor: 'primary.main',
              color: '#fff',
              fontWeight: 600,
              fontSize: '0.7rem',
            }}
          />
        )}
      </Box>

      {/* Favorite Button */}
      <IconButton
        onClick={toggleFavorite}
        sx={{
          position: 'absolute',
          top: 8,
          right: 8,
          zIndex: 1,
          bgcolor: 'rgba(255, 255, 255, 0.9)',
          '&:hover': { bgcolor: 'rgba(255, 255, 255, 1)' },
        }}
      >
        {isFavorite ? (
          <FavoriteIcon sx={{ color: 'error.main' }} />
        ) : (
          <FavoriteBorderIcon />
        )}
      </IconButton>

      {/* Product Image */}
      <CardMedia
        component="img"
        height="200"
        image={imageError ? 'https://via.placeholder.com/300x200?text=No+Image' : (product.imageUrl || 'https://via.placeholder.com/300x200?text=Product')}
        alt={product.name}
        onError={() => setImageError(true)}
        sx={{ 
          objectFit: 'contain',
          p: 2,
          bgcolor: '#F9F9F9',
        }}
      />

      {/* Product Details */}
      <CardContent sx={{ flexGrow: 1, pb: 1, pt: 2 }}>
        {/* Brand */}
        {product.brand && (
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5, fontWeight: 500 }}>
            {product.brand.name}
          </Typography>
        )}

        {/* Product Name */}
        <Typography 
          variant="subtitle1" 
          component="div" 
          fontWeight={600}
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            lineHeight: 1.3,
            minHeight: '2.6em',
            mb: 1,
            color: 'text.primary',
          }}
        >
          {product.name}
        </Typography>

        {/* Weight/Unit */}
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
          {product.weight} {product.unit}
        </Typography>

        {/* Price Section */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <Typography variant="h6" color="text.primary" fontWeight={700}>
            ${price.toFixed(2)}
          </Typography>
          {hasDiscount && (
            <Typography
              variant="body2"
              sx={{ 
                textDecoration: 'line-through', 
                color: 'text.disabled',
                fontSize: '0.875rem',
              }}
            >
              ${product.price.toFixed(2)}
            </Typography>
          )}
        </Box>

        {/* Stock Status */}
        {isOutOfStock && (
          <Chip 
            label="Out of Stock" 
            size="small" 
            color="error" 
            sx={{ fontWeight: 600 }} 
          />
        )}
        {isLowStock && !isOutOfStock && (
          <Typography variant="caption" color="warning.main" fontWeight={600}>
            Only {product.stockQuantity} left!
          </Typography>
        )}
      </CardContent>

      {/* Add to Cart Button */}
      <CardActions sx={{ pt: 0, px: 2, pb: 2 }}>
        {quantity === 0 ? (
          <Button
            fullWidth
            variant="contained"
            startIcon={<AddShoppingCartIcon />}
            onClick={handleAddToCart}
            disabled={isOutOfStock}
            sx={{
              fontWeight: 600,
              py: 1,
              bgcolor: 'primary.main',
              '&:hover': {
                bgcolor: 'primary.dark',
              },
            }}
          >
            Add to Cart
          </Button>
        ) : (
          <Box
            sx={{
              width: '100%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              border: '2px solid',
              borderColor: 'primary.main',
              borderRadius: 1,
              px: 1,
            }}
          >
            <IconButton
              size="small"
              onClick={(e) => handleQuantityChange(e, -1)}
              sx={{ color: 'primary.main' }}
            >
              <RemoveIcon />
            </IconButton>
            <Typography variant="body1" fontWeight={700}>
              {quantity}
            </Typography>
            <IconButton
              size="small"
              onClick={(e) => handleQuantityChange(e, 1)}
              disabled={quantity >= product.stockQuantity}
              sx={{ color: 'primary.main' }}
            >
              <AddIcon />
            </IconButton>
          </Box>
        )}
      </CardActions>
    </Card>
  );
}

export default ProductCard;
