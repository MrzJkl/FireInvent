import 'package:fireinvent/models/clothing_variant_model.dart';
import 'package:fireinvent/services/clothing_variant_service.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

class ClothingVariantListScreen extends StatefulWidget {
  const ClothingVariantListScreen({super.key, required this.productId});

  final String productId;

  @override
  State<ClothingVariantListScreen> createState() =>
      _ClothingVariantListScreenState();
}

class _ClothingVariantListScreenState extends State<ClothingVariantListScreen> {
  late Future<List<ClothingVariantModel>> _variantsFuture;
  final ClothingVariantService _service = GetIt.I<ClothingVariantService>();

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  void _loadData() {
    setState(() {
      _variantsFuture = _service.getAll();
    });
  }

  void _delete(String id) async {
    await _service.delete(id);
    _loadData();
  }

  @override
  Widget build(BuildContext context) {
    return const Placeholder();
  }
}
