import { Box, Typography, Button, Container } from '@mui/material';
import { ArrowForward as ArrowForwardIcon } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

interface HeroBannerProps {
  title: string;
  subtitle: string;
  buttonText?: string;
  buttonLink?: string;
  backgroundImage?: string;
  backgroundColor?: string;
  overlayColor?: string;
}

export function HeroBanner({
  title,
  subtitle,
  buttonText = 'Shop Now',
  buttonLink = '/catalog',
  backgroundImage,
  backgroundColor = 'orange',
  overlayColor = 'rgba(0,0,0,0.18)',
}: HeroBannerProps) {
  const navigate = useNavigate();

  return (
    <Box
      sx={{
        position: 'relative',
        // use background to support gradients as well as solid colors
        background: backgroundColor,
        backgroundImage: backgroundImage ? `url(${backgroundImage})` : undefined,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        color: 'white',
        py: { xs: 6, md: 10 },
        overflow: 'hidden',
      }}
    >
      {/* Overlay */}
      {backgroundImage && (
        <Box
          sx={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            bgcolor: overlayColor,
          }}
        />
      )}

      <Container maxWidth="lg" sx={{ position: 'relative', zIndex: 1 }}>
        <Box
          sx={{
            maxWidth: 600,
            textAlign: { xs: 'center', md: 'left' },
          }}
        >
          <Typography
            variant="h2"
            fontWeight={700}
            gutterBottom
            sx={{
              fontSize: { xs: '2rem', md: '3rem' },
              textShadow: backgroundImage ? '2px 2px 4px rgba(0,0,0,0.3)' : 'none',
            }}
          >
            {title}
          </Typography>
          <Typography
            variant="h6"
            sx={{
              mb: 4,
              opacity: 0.95,
              textShadow: backgroundImage ? '1px 1px 2px rgba(0,0,0,0.3)' : 'none',
            }}
          >
            {subtitle}
          </Typography>
          <Button
            variant="contained"
            size="large"
            endIcon={<ArrowForwardIcon />}
            onClick={() => navigate(buttonLink)}
            sx={{
              bgcolor: 'white',
              color: 'primary.main',
              px: 4,
              py: 1.5,
              fontSize: '1.1rem',
              fontWeight: 700,
              '&:hover': {
                bgcolor: 'grey.100',
                transform: 'scale(1.05)',
              },
            }}
          >
            {buttonText}
          </Button>
        </Box>
      </Container>
    </Box>
  );
}

export default HeroBanner;
