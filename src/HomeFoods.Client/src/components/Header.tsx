import { AppBar, Toolbar, Typography, IconButton, Badge, Box, Button, Container } from '@mui/material';
import { ShoppingCart as ShoppingCartIcon, Home as HomeIcon, Storefront as StorefrontIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import SearchBar from './SearchBar';

function Header() {
  const navigate = useNavigate();
  const { getTotalItems } = useCart();

  return (
    <AppBar 
      position="sticky" 
      elevation={1}
      sx={{ 
        bgcolor: 'white',
        color: 'text.primary',
      }}
    >
      <Container maxWidth="xl">
        <Toolbar sx={{ gap: 2, py: 1 }}>
          {/* Logo */}
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              cursor: 'pointer',
              mr: 2,
            }}
            onClick={() => navigate('/')}
          >
            <StorefrontIcon sx={{ fontSize: 32, color: 'primary.main', mr: 1 }} />
            <Typography
              variant="h6"
              component="div"
              sx={{ 
                fontWeight: 700,
                color: 'primary.main',
                display: { xs: 'none', sm: 'block' },
              }}
            >
              HomeFoods
            </Typography>
          </Box>

          {/* Navigation Links */}
          <Box sx={{ display: { xs: 'none', md: 'flex' }, gap: 1 }}>
            <Button 
              color="inherit" 
              onClick={() => navigate('/')}
              sx={{ fontWeight: 600 }}
            >
              Home
            </Button>
            <Button 
              color="inherit" 
              onClick={() => navigate('/catalog')}
              sx={{ fontWeight: 600 }}
            >
              Shop
            </Button>
            <Button
              color="inherit"
              onClick={() => navigate('/chat')}
              sx={{ fontWeight: 600 }}
            >
              Chat
            </Button>
          </Box>

          {/* Search Bar */}
          <Box sx={{ flexGrow: 1, maxWidth: 600, display: { xs: 'none', md: 'block' } }}>
            <SearchBar />
          </Box>

          {/* Cart Icon */}
          <IconButton
            color="primary"
            onClick={() => navigate('/cart')}
            aria-label="shopping cart"
            sx={{
              bgcolor: 'primary.light',
              color: 'white',
              '&:hover': {
                bgcolor: 'primary.main',
              },
            }}
          >
            <Badge 
              badgeContent={getTotalItems()} 
              color="secondary"
              sx={{
                '& .MuiBadge-badge': {
                  backgroundColor: 'secondary.main',
                  color: '#000',
                  fontWeight: 700,
                },
              }}
            >
              <ShoppingCartIcon />
            </Badge>
          </IconButton>
        </Toolbar>

        {/* Mobile Search Bar */}
        <Box sx={{ display: { xs: 'block', md: 'none' }, pb: 2, px: 2 }}>
          <SearchBar fullWidth />
        </Box>
      </Container>
    </AppBar>
  );
}

export default Header;
