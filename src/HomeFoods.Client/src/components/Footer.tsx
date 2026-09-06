import { Box, Container, Grid, Typography, Link, IconButton } from '@mui/material';
import { Facebook, Twitter, Instagram } from '@mui/icons-material';

function Footer() {
  return (
    <Box
      component="footer"
      sx={{
        mt: 6,
        py: 4,
        // warm gradient blending brand green and orange
        background: 'linear-gradient(90deg, #84C225 0%, #A6CE39 30%, #FFB74D 70%, #FF9800 100%)',
        color: 'common.white',
      }}
    >
      <Container maxWidth="xl">
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} md={4}>
            <Typography variant="h6" fontWeight={700} sx={{ color: 'common.white' }}>
              HomeFoods
            </Typography>
            <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.9)' }}>
              Fresh groceries delivered to your doorstep.
            </Typography>
          </Grid>

          <Grid item xs={12} md={4}>
            <Typography variant="subtitle1" fontWeight={700} sx={{ color: 'common.white' }}>
              Quick Links
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', mt: 1 }}>
              <Link href="/catalog" underline="none" sx={{ mb: 0.5, color: 'rgba(255,255,255,0.95)' }}>
                Shop
              </Link>
              <Link href="/about" underline="none" sx={{ mb: 0.5, color: 'rgba(255,255,255,0.95)' }}>
                About
              </Link>
              <Link href="/contact" underline="none" sx={{ color: 'rgba(255,255,255,0.95)' }}>
                Contact
              </Link>
            </Box>
          </Grid>

          <Grid item xs={12} md={4}>
            <Typography variant="subtitle1" fontWeight={700} sx={{ color: 'common.white' }}>
              Follow Us
            </Typography>
            <Box sx={{ mt: 1 }}>
              <IconButton href="#" aria-label="facebook" sx={{ color: 'common.white' }}>
                <Facebook />
              </IconButton>
              <IconButton href="#" aria-label="twitter" sx={{ color: 'common.white' }}>
                <Twitter />
              </IconButton>
              <IconButton href="#" aria-label="instagram" sx={{ color: 'common.white' }}>
                <Instagram />
              </IconButton>
            </Box>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.85)' }}>
              © {new Date().getFullYear()} HomeFoods. All rights reserved.
            </Typography>
          </Grid>
        </Grid>
      </Container>
    </Box>
  );
}

export default Footer;
