import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
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
  final _formKey = GlobalKey<FormBuilderState>();

  @override
  Widget build(BuildContext context) {
    return FormBuilder(
      key: _formKey,
      initialValue: {
        'name': widget.initial?.name ?? '',
        'manufacturer': widget.initial?.manufacturer ?? '',
        'description': widget.initial?.description ?? '',
        'type': widget.initial?.type ?? GearType.jacket,
      },
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          FormBuilderTextField(
            name: 'name',
            decoration: const InputDecoration(labelText: 'Name'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'manufacturer',
            decoration: const InputDecoration(labelText: 'Manufacturer'),
            validator: FormBuilderValidators.compose([
              FormBuilderValidators.required(),
            ]),
          ),
          FormBuilderTextField(
            name: 'description',
            decoration: const InputDecoration(labelText: 'Description'),
          ),
          FormBuilderDropdown<GearType>(
            name: 'type',
            decoration: const InputDecoration(labelText: 'Type'),
            initialValue: widget.initial?.type ?? GearType.jacket,
            items:
                GearType.values
                    .map((g) => DropdownMenuItem(value: g, child: Text(g.name)))
                    .toList(),
            validator: FormBuilderValidators.required(),
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
                  if (_formKey.currentState?.saveAndValidate() ?? false) {
                    final values = _formKey.currentState!.value;
                    widget.onSubmit(
                      CreateClothingProductModel(
                        name: values['name'] as String,
                        manufacturer: values['manufacturer'] as String,
                        description: values['description'] as String,
                        type: values['type'] as GearType,
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
