import { useState, useEffect } from 'react';
import { Container, Box, Typography, Grid, Button } from '@mui/material';
import { ArrowForward as ArrowForwardIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { Product, Category } from '../types';
import { productService, categoryService } from '../services/api';
import HeroBanner from '../components/HeroBanner';
import CategoryGrid from '../components/CategoryGrid';
import ProductCarousel from '../components/ProductCarousel';
import LoadingState from '../components/LoadingState';

function HomePage() {
  const navigate = useNavigate();
  const [featuredProducts, setFeaturedProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const [productsData, categoriesData] = await Promise.all([
          productService.getFeatured(),
          categoryService.getAll(),
        ]);

        setFeaturedProducts(productsData);
        setCategories(categoriesData);
      } catch (error) {
        console.error('Error fetching data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return <LoadingState message="Loading fresh deals..." />;
  }

  return (
    <Box>
      {/* Hero Banner */}
      <HeroBanner
        title="Fresh Groceries Delivered to Your Doorstep"
        subtitle="Get farm-fresh produce, dairy, and more with great deals every day!"
        buttonText="Start Shopping"
        buttonLink="/catalog"
        backgroundColor="linear-gradient(135deg, #FF9800 0%, #FF5722 100%)"
      />

      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Categories */}
        <CategoryGrid categories={categories} />

        {/*Featured Products */}
        {featuredProducts.length > 0 && (
          <ProductCarousel
            title="Featured Products"
            subtitle="Hand-picked products just for you"
            products={featuredProducts}
          />
        )}

        {/* Deals Section */}
        <Box
          sx={{
            bgcolor: 'primary.light',
            borderRadius: 3,
            p: 4,
            my: 4,
            textAlign: 'center',
            color: 'white',
          }}
        >
          <Typography variant="h4" fontWeight={700} gutterBottom>
            🎉 Special Offers Today!
          </Typography>
          <Typography variant="h6" sx={{ mb: 3, opacity: 0.95 }}>
            Save up to 50% on selected items
          </Typography>
          <Button
            variant="contained"
            size="large"
            endIcon={<ArrowForwardIcon />}
            onClick={() => navigate('/catalog?minDiscount=20')}
            sx={{
              bgcolor: 'white',
              color: 'primary.main',
              px: 4,
              py: 1.5,
              fontWeight: 700,
              '&:hover': {
                bgcolor: 'grey.100',
              },
            }}
          >
            View All Deals
          </Button>
        </Box>

        {/* Why Choose Us */}
        <Box sx={{ py: 4 }}>
          <Typography variant="h5" fontWeight={700} textAlign="center" gutterBottom>
            Why Choose HomeFoods?
          </Typography>
          <Grid container spacing={4} sx={{ mt: 2 }}>
            <Grid item xs={12} md={4}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h3" sx={{ mb: 2 }}>
                  🚚
                </Typography>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Fast Delivery
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Get your groceries delivered within hours
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h3" sx={{ mb: 2 }}>
                  🌿
                </Typography>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Fresh Products
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Farm-fresh produce and quality guaranteed
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h3" sx={{ mb: 2 }}>
                  💰
                </Typography>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Great Prices
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Best deals and offers on all products
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </Box>
      </Container>
    </Box>
  );
}

export default HomePage;
