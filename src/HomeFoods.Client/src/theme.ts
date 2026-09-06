import { createTheme } from '@mui/material/styles';

// BigBasket-inspired color palette
export const theme = createTheme({
  palette: {
    primary: {
      main: '#84C225', // BigBasket Green
      light: '#A5D45B',
      dark: '#6FA01C',
      contrastText: '#fff',
    },
    secondary: {
      main: '#F8B500', // Golden Yellow for offers
      light: '#FFD54F',
      dark: '#C68400',
      contrastText: '#000',
    },
    error: {
      main: '#F44336',
    },
    warning: {
      main: '#FF9800',
    },
    info: {
      main: '#2196F3',
    },
    success: {
      main: '#4CAF50',
    },
    background: {
      default: '#F4F6F9',
      paper: '#ffffff',
    },
    text: {
      primary: '#2C3E50',
      secondary: '#546E7A',
    },
  },
  typography: {
    fontFamily: '"Helvetica Neue", "Roboto", "Arial", sans-serif',
    h1: {
      fontWeight: 700,
      fontSize: '2.5rem',
      letterSpacing: '-0.02em',
    },
    h2: {
      fontWeight: 700,
      fontSize: '2rem',
      letterSpacing: '-0.01em',
    },
    h3: {
      fontWeight: 600,
      fontSize: '1.75rem',
    },
    h4: {
      fontWeight: 600,
      fontSize: '1.5rem',
    },
    h5: {
      fontWeight: 600,
      fontSize: '1.25rem',
    },
    h6: {
      fontWeight: 600,
      fontSize: '1rem',
    },
    subtitle1: {
      fontSize: '1rem',
      fontWeight: 500,
    },
    subtitle2: {
      fontSize: '0.875rem',
      fontWeight: 500,
    },
    body1: {
      fontSize: '0.95rem',
      lineHeight: 1.6,
    },
    body2: {
      fontSize: '0.875rem',
      lineHeight: 1.5,
    },
    button: {
      fontWeight: 600,
      textTransform: 'none',
      fontSize: '0.95rem',
    },
    caption: {
      fontSize: '0.75rem',
      color: '#757575',
    },
  },
  shape: {
    borderRadius: 8,
  },
  shadows: [
    'none',
    '0px 2px 4px rgba(0,0,0,0.05)',
    '0px 2px 8px rgba(0,0,0,0.08)',
    '0px 4px 12px rgba(0,0,0,0.1)',
    '0px 6px 16px rgba(0,0,0,0.12)',
    '0px 8px 20px rgba(0,0,0,0.14)',
    '0px 10px 24px rgba(0,0,0,0.16)',
    '0px 12px 28px rgba(0,0,0,0.18)',
    '0px 14px 32px rgba(0,0,0,0.20)',
    '0px 16px 36px rgba(0,0,0,0.22)',
    '0px 18px 40px rgba(0,0,0,0.24)',
    '0px 20px 44px rgba(0,0,0,0.26)',
    '0px 22px 48px rgba(0,0,0,0.28)',
    '0px 24px 52px rgba(0,0,0,0.30)',
    '0px 26px 56px rgba(0,0,0,0.32)',
    '0px 28px 60px rgba(0,0,0,0.34)',
    '0px 30px 64px rgba(0,0,0,0.36)',
    '0px 32px 68px rgba(0,0,0,0.38)',
    '0px 34px 72px rgba(0,0,0,0.40)',
    '0px 36px 76px rgba(0,0,0,0.42)',
    '0px 38px 80px rgba(0,0,0,0.44)',
    '0px 40px 84px rgba(0,0,0,0.46)',
    '0px 42px 88px rgba(0,0,0,0.48)',
    '0px 44px 92px rgba(0,0,0,0.50)',
    '0px 46px 96px rgba(0,0,0,0.52)',
  ],
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 6,
          padding: '10px 24px',
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0px 4px 12px rgba(0,0,0,0.15)',
          },
        },
        contained: {
          boxShadow: '0px 2px 8px rgba(0,0,0,0.1)',
        },
        containedPrimary: {
          '&:hover': {
            backgroundColor: '#6FA01C',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0px 2px 8px rgba(0,0,0,0.06)',
          transition: 'box-shadow 0.3s ease, transform 0.2s ease',
          '&:hover': {
            boxShadow: '0px 4px 16px rgba(0,0,0,0.12)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 6,
          fontWeight: 500,
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 8,
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
        elevation1: {
          boxShadow: '0px 2px 8px rgba(0,0,0,0.06)',
        },
      },
    },
  },
});

export default theme;
