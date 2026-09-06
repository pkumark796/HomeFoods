import { Box, Card, CardContent, Typography, Grid } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { Category } from '../types';

interface CategoryGridProps {
  categories: Category[];
}

export function CategoryGrid({ categories }: CategoryGridProps) {
  const navigate = useNavigate();

  const handleCategoryClick = (categoryId: string) => {
    navigate(`/catalog?categoryId=${categoryId}`);
  };

  return (
    <Box sx={{ py: 4 }}>
      <Typography variant="h5" fontWeight={700} gutterBottom sx={{ mb: 3 }}>
        Shop by Category
      </Typography>

      <Grid container spacing={2}>
        {categories.map((category) => (
          <Grid item xs={6} sm={4} md={3} lg={2} key={category.id}>
            <Card
              onClick={() => handleCategoryClick(category.id)}
              sx={{
                cursor: 'pointer',
                transition: 'all 0.3s ease',
                height: '100%',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: '0 8px 24px rgba(132, 194, 37, 0.2)',
                },
              }}
            >
              <Box
                sx={{
                  height: 120,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: 'background.default',
                  backgroundImage: category.imageUrl ? `url(${category.imageUrl})` : 'none',
                  backgroundSize: 'cover',
                  backgroundPosition: 'center',
                  p: 2,
                }}
              >
                {!category.imageUrl && (
                  <Typography variant="h3" sx={{ opacity: 0.1 }}>
                    📦
                  </Typography>
                )}
              </Box>
              <CardContent sx={{ textAlign: 'center', py: 1.5 }}>
                <Typography
                  variant="subtitle2"
                  fontWeight={600}
                  sx={{
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    display: '-webkit-box',
                    WebkitLineClamp: 2,
                    WebkitBoxOrient: 'vertical',
                  }}
                >
                  {category.name}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}

export default CategoryGrid;
