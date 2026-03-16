import 'package:flutter/material.dart';

const Color _primaryColor = Color(0xFF6C63FF);
const Color _secondaryColor = Color(0xFFFF6584);
const Color _surfaceColor = Color(0xFFF5F5F5);
const Color _darkSurfaceColor = Color(0xFF1E1E2C);
const Color _darkBackgroundColor = Color(0xFF14142B);

final ThemeData musicraticTheme = ThemeData(
  useMaterial3: true,
  colorScheme: ColorScheme.fromSeed(
    seedColor: _primaryColor,
    secondary: _secondaryColor,
    surface: _surfaceColor,
  ),
  fontFamily: 'Roboto',
  appBarTheme: const AppBarTheme(
    centerTitle: true,
    elevation: 0,
  ),
  cardTheme: CardTheme(
    elevation: 2,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(12),
    ),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
      ),
    ),
  ),
);

final ThemeData musicraticDarkTheme = ThemeData(
  useMaterial3: true,
  brightness: Brightness.dark,
  colorScheme: ColorScheme.fromSeed(
    seedColor: _primaryColor,
    secondary: _secondaryColor,
    brightness: Brightness.dark,
    surface: _darkSurfaceColor,
  ),
  scaffoldBackgroundColor: _darkBackgroundColor,
  fontFamily: 'Roboto',
  appBarTheme: const AppBarTheme(
    centerTitle: true,
    elevation: 0,
  ),
  cardTheme: CardTheme(
    elevation: 2,
    shape: RoundedRectangleBorder(
      borderRadius: BorderRadius.circular(12),
    ),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
      ),
    ),
  ),
);
