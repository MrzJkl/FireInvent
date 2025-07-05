import 'package:flutter/material.dart';
import '../models/create_models/create_clothing_product_model.dart';
import '../models/clothing_product_model.dart';
import '../models/gear_type.dart';

class ClothingProductForm extends StatefulWidget {
  final ClothingProductModel? initial;
  final void Function(CreateClothingProductModel) onSubmit;

  const ClothingProductForm({super.key, this.initial, required this.onSubmit});

  @override
  State<ClothingProductForm> createState() => _ClothingProductFormState();
}

class _ClothingProductFormState extends State<ClothingProductForm> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _nameController;
  late TextEditingController _manufacturerController;
  late TextEditingController _descriptionController;
  late GearType _type;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.initial?.name ?? '');
    _manufacturerController = TextEditingController(
      text: widget.initial?.manufacturer ?? '',
    );
    _descriptionController = TextEditingController(
      text: widget.initial?.description ?? '',
    );
    _type = widget.initial?.type ?? GearType.jacket;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _manufacturerController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: _formKey,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextFormField(
            controller: _nameController,
            decoration: const InputDecoration(labelText: 'Name'),
            validator:
                (value) => value == null || value.isEmpty ? 'Required' : null,
          ),
          TextFormField(
            controller: _manufacturerController,
            decoration: const InputDecoration(labelText: 'Manufacturer'),
            validator:
                (value) => value == null || value.isEmpty ? 'Required' : null,
          ),
          TextFormField(
            controller: _descriptionController,
            decoration: const InputDecoration(labelText: 'Description'),
          ),
          DropdownButtonFormField<GearType>(
            value: _type,
            items:
                GearType.values
                    .map((g) => DropdownMenuItem(value: g, child: Text(g.name)))
                    .toList(),
            onChanged: (val) => setState(() => _type = val!),
            decoration: const InputDecoration(labelText: 'Type'),
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Cancel'),
              ),
              ElevatedButton(
                onPressed: () {
                  if (_formKey.currentState?.validate() ?? false) {
                    widget.onSubmit(
                      CreateClothingProductModel(
                        name: _nameController.text,
                        manufacturer: _manufacturerController.text,
                        description: _descriptionController.text,
                        type: _type,
                      ),
                    );
                  }
                },
                child: const Text('Save'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
