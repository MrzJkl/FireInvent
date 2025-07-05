import 'package:flameguardlaundry/constants.dart';
import 'package:flameguardlaundry/main_drawer.dart';
import 'package:flameguardlaundry/models/create_models/create_clothing_product_model.dart';
import 'package:flameguardlaundry/models/gear_type.dart';
import 'package:flameguardlaundry/widgets/clothing_products_table.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';
import '../models/clothing_product_model.dart';
import '../services/clothing_product_service.dart';

class ClothingProductListScreen extends StatefulWidget {
  const ClothingProductListScreen({super.key});

  @override
  State<ClothingProductListScreen> createState() =>
      _ClothingProductListScreenState();
}

class _ClothingProductListScreenState extends State<ClothingProductListScreen> {
  late Future<List<ClothingProductModel>> _productsFuture;
  final ClothingProductService _service = GetIt.I<ClothingProductService>();

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  void _loadData() {
    setState(() {
      _productsFuture = _service.getAll();
    });
  }

  void _delete(String id) async {
    await _service.delete(id);
    _loadData();
  }

  Future<void> showClothingProductFormDialog({
    required BuildContext context,
    ClothingProductModel? initial,
    required void Function(String? id, CreateClothingProductModel) onSubmit,
  }) async {
    final formKey = GlobalKey<FormState>();
    final nameController = TextEditingController(text: initial?.name ?? '');
    final manufacturerController = TextEditingController(
      text: initial?.manufacturer ?? '',
    );
    final descriptionController = TextEditingController(
      text: initial?.description ?? '',
    );
    GearType type = initial?.type ?? GearType.jacket;

    await showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: Text(initial == null ? 'Create Product' : 'Edit Product'),
            content: SizedBox(
              width: 400,
              child: Form(
                key: formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    TextFormField(
                      controller: nameController,
                      decoration: const InputDecoration(labelText: 'Name'),
                      validator:
                          (value) =>
                              value == null || value.isEmpty
                                  ? 'Required'
                                  : null,
                    ),
                    TextFormField(
                      controller: manufacturerController,
                      decoration: const InputDecoration(
                        labelText: 'Manufacturer',
                      ),
                      validator:
                          (value) =>
                              value == null || value.isEmpty
                                  ? 'Required'
                                  : null,
                    ),
                    TextFormField(
                      controller: descriptionController,
                      decoration: const InputDecoration(
                        labelText: 'Description',
                      ),
                    ),
                    DropdownButtonFormField<GearType>(
                      value: type,
                      items:
                          GearType.values
                              .map(
                                (g) => DropdownMenuItem(
                                  value: g,
                                  child: Text(g.name),
                                ),
                              )
                              .toList(),
                      onChanged: (val) => type = val!,
                      decoration: const InputDecoration(labelText: 'Type'),
                    ),
                  ],
                ),
              ),
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Cancel'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (formKey.currentState?.validate() ?? false) {
                    final createModel = CreateClothingProductModel(
                      name: nameController.text,
                      manufacturer: manufacturerController.text,
                      description: descriptionController.text,
                      type: type,
                    );
                    Navigator.of(context).pop();
                    onSubmit(initial?.id, createModel);
                  }
                },
                child: const Text('Save'),
              ),
            ],
          ),
    );
  }

  void _edit(ClothingProductModel model) async {
    await showClothingProductFormDialog(
      context: context,
      initial: model,
      onSubmit: (id, updated) async {
        final updatedModel = ClothingProductModel(
          id: id!,
          name: updated.name,
          manufacturer: updated.manufacturer,
          description: updated.description,
          type: updated.type,
        );
        await _service.update(id, updatedModel);
        _loadData();
      },
    );
  }

  void _create() async {
    await showClothingProductFormDialog(
      context: context,
      onSubmit: (id, created) async {
        await _service.create(created);
        _loadData();
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: Constants.buildMainAppBar(context),
      drawer: MainDrawer(),
      floatingActionButton: FloatingActionButton(
        onPressed: _create,
        tooltip: 'Add Product',
        child: const Icon(Icons.add),
      ),
      body: FutureBuilder<List<ClothingProductModel>>(
        future: _productsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No products available.'));
          }

          final products = snapshot.data!;

          return ClothingProductTable(
            products: products,
            onEdit: _edit,
            onDelete: _delete,
          );
        },
      ),
    );
  }
}
